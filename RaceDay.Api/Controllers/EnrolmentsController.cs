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
    [RequireRole]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EnrolmentsController(RaceDayDbContext db) => _db = db;

        // POST: api/enrolments (Participant only)
        [HttpPost]
        [RequireRole("Participant")]
        public async Task<IActionResult> CreateEnrolment([FromBody] CreateEnrolmentRequest request)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var ev = await _db.Events.FindAsync(request.EventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var cat = await _db.Categories.FindAsync(request.CategoryId);
            if (cat == null || cat.EventId != request.EventId)
                return BadRequest(new { message = "Invalid category for this event." });

            // Prevent duplicate
            if (await _db.Enrolments.AnyAsync(e => e.ParticipantId == userId
                && e.EventId == request.EventId && e.Status != "Cancelled"))
                return BadRequest(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantId = userId,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                Status = "Pending",
                EnrolmentDate = DateTime.UtcNow
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { enrolment.EnrolmentId, message = "Enrolment created." });
        }

        // GET: api/enrolments/mine (Participant only)
        [HttpGet("mine")]
        [RequireRole("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var list = await _db.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Participant)
                .Where(e => e.ParticipantId == userId)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName,
                    EventId = e.EventId,
                    EventName = e.Event!.Name,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category!.Name,
                    Status = e.Status,
                    EnrolmentDate = e.EnrolmentDate,
                    BibNumber = e.BibNumber
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/enrolments/event/5 (Organiser only)
        [HttpGet("event/{eventId}")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var ev = await _db.Events.FindAsync(eventId);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            var list = await _db.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Participant)
                .Where(e => e.EventId == eventId)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName,
                    EventId = e.EventId,
                    EventName = e.Event!.Name,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category!.Name,
                    Status = e.Status,
                    EnrolmentDate = e.EnrolmentDate,
                    BibNumber = e.BibNumber
                })
                .ToListAsync();

            return Ok(list);
        }

        // PUT: api/enrolments/5/status (Organiser only)
        [HttpPut("{id}/status")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusRequest request)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Invalid status." });

            var enrolment = await _db.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event!.OrganiserId != userId)
                return StatusCode(403, new { message = "You do not own this event." });

            enrolment.Status = request.Status;

            // Assign bib number on confirm
            if (request.Status == "Confirmed" && string.IsNullOrEmpty(enrolment.BibNumber))
                enrolment.BibNumber = $"BIB{enrolment.EnrolmentId:D5}";

            await _db.SaveChangesAsync();
            return Ok(new { message = "Status updated.", enrolment.BibNumber });
        }
    }
}