using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [RequireRole]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public CategoriesController(RaceDayDbContext db) => _db = db;

        // GET: api/events/5/categories
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            var cats = await _db.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    MinAge = c.MinAge,
                    MaxAge = c.MaxAge,
                    EntryFee = c.EntryFee,
                    EventId = c.EventId
                })
                .ToListAsync();

            return Ok(cats);
        }

        // POST: api/events/5/categories
        [HttpPost("events/{eventId}/categories")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var ev = await _db.Events.FindAsync(eventId);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            var cat = new Category
            {
                Name = request.Name,
                Description = request.Description,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                EntryFee = request.EntryFee,
                EventId = eventId
            };

            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { cat.CategoryId, message = "Category created." });
        }

        // PUT: api/categories/5
        [HttpPut("categories/{id}")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null) return NotFound(new { message = "Category not found." });
            if (cat.Event!.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            cat.Name = request.Name;
            cat.Description = request.Description;
            cat.MinAge = request.MinAge;
            cat.MaxAge = request.MaxAge;
            cat.EntryFee = request.EntryFee;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Category updated." });
        }

        // DELETE: api/categories/5
        [HttpDelete("categories/{id}")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null) return NotFound(new { message = "Category not found." });
            if (cat.Event!.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Category deleted." });
        }
    }
}
