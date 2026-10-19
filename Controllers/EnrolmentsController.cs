

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/Enrolments/my
    // Participants can view their own enrolments.
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantId == participantId)
            .Select(e => new
            {
                e.Id,
                EventId = e.Category != null ? e.Category.EventId : 0,
                EventName = e.Category != null &&
                            e.Category.Event != null
                    ? e.Category.Event.Name
                    : "",
                e.CategoryId,
                CategoryName = e.Category != null
                    ? e.Category.Name
                    : "",
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // POST: api/Enrolments
    // Participants can enrol in an event and category.
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrolment(
        EnrolmentRequest request)
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        if (request.EventId <= 0 || request.CategoryId <= 0)
            return BadRequest(
                "Valid event and category IDs are required.");

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (eventItem.EventDate <= DateTime.UtcNow)
            return BadRequest(
                "You cannot enrol in an event that has already started.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId);

        if (category == null)
            return NotFound("Category not found.");

        if (category.EventId != request.EventId)
            return BadRequest(
                "The selected category does not belong to this event.");

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e =>
                e.ParticipantId == participantId &&
                e.Category != null &&
                e.Category.EventId == request.EventId &&
                e.Status != "Cancelled");

        if (alreadyEnrolled)
            return Conflict(
                "You are already enrolled in this event.");

        var activeEnrolments = await _context.Enrolments
            .CountAsync(e =>
                e.CategoryId == request.CategoryId &&
                e.Status != "Cancelled");

        if (activeEnrolments >= category.MaxParticipants)
            return Conflict(
                "This category has reached its participant limit.");

        var enrolment = new Enrolment
        {
            ParticipantId = participantId,
            CategoryId = request.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Active"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMyEnrolments),
            new { id = enrolment.Id },
            new
            {
                enrolment.Id,
                enrolment.ParticipantId,
                EventId = category.EventId,
                enrolment.CategoryId,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
    }

    // GET: api/Enrolments/event/5
    // Organisers can view enrolments for their own events.
    [HttpGet("event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var eventItem = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (eventItem.OrganiserId != organiserId)
            return Forbid();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.Category != null &&
                        e.Category.EventId == eventId)
            .Select(e => new
            {
                e.Id,
                e.ParticipantId,
                ParticipantName = e.Participant != null
                    ? e.Participant.FirstName + " " +
                      e.Participant.LastName
                    : "",
                e.CategoryId,
                CategoryName = e.Category != null
                    ? e.Category.Name
                    : "",
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // DELETE: api/Enrolments/5
    // Participants can cancel only their own enrolments.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var enrolment = await _context.Enrolments
            .FirstOrDefaultAsync(e =>
                e.Id == id &&
                e.ParticipantId == participantId);

        if (enrolment == null)
            return NotFound("Enrolment not found.");

        if (enrolment.Status == "Cancelled")
            return BadRequest(
                "This enrolment has already been cancelled.");

        enrolment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment cancelled successfully.",
            enrolment.Id,
            enrolment.Status
        });
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId);
    }
}

// Data accepted when creating an enrolment.
public class EnrolmentRequest
{
    public int EventId { get; set; }

    public int CategoryId { get; set; }
}
