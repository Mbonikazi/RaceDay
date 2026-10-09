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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ResultsController(RaceDayDbContext db) => _db = db;

        // POST: api/results (Organiser only)
        [HttpPost]
        [RequireRole("Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var enrolment = await _db.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event!.OrganiserId != userId)
                return StatusCode(403, new { message = "You do not own this event." });

            if (await _db.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
                return BadRequest(new { message = "Result already exists for this enrolment." });

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition,
                RecordedAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { result.ResultId, message = "Result recorded." });
        }

        // GET: api/results/mine (Participant only)
        [HttpGet("mine")]
        [RequireRole("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var list = await _db.Results
                .Include(r => r.Enrolment)!.ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)!.ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)!.ThenInclude(e => e.Category)
                .Where(r => r.Enrolment!.ParticipantId == userId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    EventName = r.Enrolment.Event!.Name,
                    CategoryName = r.Enrolment.Category!.Name,
                    FinishTime = r.FinishTime,
                    FinishingPosition = r.FinishingPosition,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/results/event/5 (Organiser only)
        [HttpGet("event/{eventId}")]
        [RequireRole("Organiser")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var ev = await _db.Events.FindAsync(eventId);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId) return StatusCode(403, new { message = "You do not own this event." });

            var list = await _db.Results
                .Include(r => r.Enrolment)!.ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)!.ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)!.ThenInclude(e => e.Category)
                .Where(r => r.Enrolment!.EventId == eventId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    EventName = r.Enrolment.Event!.Name,
                    CategoryName = r.Enrolment.Category!.Name,
                    FinishTime = r.FinishTime,
                    FinishingPosition = r.FinishingPosition,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
