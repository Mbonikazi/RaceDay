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
    [Route("api/[controller]")]
    [RequireRole] // any logged-in user
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EventsController(RaceDayDbContext db) => _db = db;

        // GET: api/events (both roles)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _db.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    EventType = e.EventType,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName,
                    Categories = e.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        MinAge = c.MinAge,
                        MaxAge = c.MaxAge,
                        EntryFee = c.EntryFee,
                        EventId = c.EventId
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await _db.Events
                .Include(ev => ev.Organiser)
                .Include(ev => ev.Categories)
                .FirstOrDefaultAsync(ev => ev.EventId == id);

            if (e == null) return NotFound(new { message = "Event not found." });

            return Ok(new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                OrganiserId = e.OrganiserId,
                OrganiserName = $"{e.Organiser!.FirstName} {e.Organiser.LastName}",
                Categories = e.Categories.Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    MinAge = c.MinAge,
                    MaxAge = c.MaxAge,
                    EntryFee = c.EntryFee,
                    EventId = c.EventId
                }).ToList()
            });
        }

        // GET: api/events/mine
        [HttpGet("mine")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var events = await _db.Events
                .Include(e => e.Categories)
                .Where(e => e.OrganiserId == userId)
                .ToListAsync();

            return Ok(events.Select(e => new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                OrganiserId = e.OrganiserId
            }));
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [RequireRole("Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var ev = new Event
            {
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                OrganiserId = userId
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, new { ev.EventId, message = "Event created." });
        }

        // PUT: api/events/5
        [HttpPut("{id}")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var ev = await _db.Events.FindAsync(id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            ev.Name = request.Name;
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.Location = request.Location;
            ev.DistanceKm = request.DistanceKm;
            ev.EventType = request.EventType;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Event updated." });
        }

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var ev = await _db.Events.FindAsync(id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Event deleted." });
        }
    }
}
