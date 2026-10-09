using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole] // just logged in
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ProfileController(RaceDayDbContext db) => _db = db;

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null) return Unauthorized();

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null) return NotFound();

            return Ok(new ProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role!.Name,
                CreatedAt = user.CreatedAt
            });
        }

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null) return Unauthorized();

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            // Check email uniqueness
            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.UserId != userId.Value))
                return BadRequest(new { message = "Email already in use." });

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}
