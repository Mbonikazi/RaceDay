using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _db;
        private readonly IRaceDayPasswordHasher _hasher;

        public AuthController(RaceDayDbContext db, IRaceDayPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // ============================================================
        // POST: api/auth/register
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Validate model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Validate role
            if (request.Role != "Organiser" && request.Role != "Participant")
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

            // 3. Check for duplicate email
            var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                return BadRequest(new { message = "Email already registered." });

            // 4. Look up the role in the database
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role == null)
                return BadRequest(new { message = "Invalid role." });

            // 5. Create the user with a hashed password
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = _hasher.Hash(request.Password),
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Save to database
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // 7. Return 201 Created
            return StatusCode(201, new AuthResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = role.Name,
                Message = "Registration successful."
            });
        }

        // ============================================================
        // POST: api/auth/login
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Validate model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Find user by email
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 3. Verify credentials
            if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            // 4. Store session data
            HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
            HttpContext.Session.SetString(SessionKeys.Role, user.Role!.Name);
            HttpContext.Session.SetString(SessionKeys.UserName, $"{user.FirstName} {user.LastName}");

            // 5. Return 200 OK with user info
            return Ok(new AuthResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.Name,
                Message = "Login successful."
            });
        }

        // ============================================================
        // POST: api/auth/logout
        // ============================================================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            return Ok(new { message = "Logged out successfully." });
        }
    }
}