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
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/Results/my
    // Participants can view results for their own enrolments.
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        if (!TryGetUserId(out int participantId))
        {
            return Unauthorized();
        }

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment != null &&
                r.Enrolment.ParticipantId == participantId)
            .Select(r => new
            {
                r.Id,
                r.EnrolmentId,
                EventName = r.Enrolment!.Event != null
                    ? r.Enrolment.Event.Name
                    : "",
                r.FinishTime,
                r.Position,
                r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/Results/event/5
    // Organisers can view results for their own events.
    [HttpGet("event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        if (!TryGetUserId(out int organiserId))
        {
            return Unauthorized();
        }

        var eventItem = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        if (eventItem.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment != null &&
                r.Enrolment.EventId == eventId)
            .Select(r => new
            {
                r.Id,
                r.EnrolmentId,
                ParticipantName = r.Enrolment!.Participant != null
                    ? r.Enrolment.Participant.FirstName + " " +
                      r.Enrolment.Participant.LastName
                    : "",
                r.FinishTime,
                r.Position,
                r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    // POST: api/Results/enrolment/5
    // Organisers can record a result for an enrolment
    // belonging to one of their own events.
    [HttpPost("enrolment/{enrolmentId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(
        int enrolmentId,
        ResultRequest request)
    {
        var error = ValidateResult(request);

        if (error != null)
        {
            return BadRequest(error);
        }

        if (!TryGetUserId(out int organiserId))
        {
            return Unauthorized();
        }

        var enrolment = await _context.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.Id == enrolmentId);

        if (enrolment == null)
        {
            return NotFound("Enrolment not found.");
        }

        if (enrolment.Event == null ||
            enrolment.Event.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var existingResult = await _context.Results
            .AnyAsync(r => r.EnrolmentId == enrolmentId);

        if (existingResult)
        {
            return Conflict(
                "A result already exists for this enrolment. Use the update endpoint.");
        }

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position,
            ResultStatus = request.ResultStatus.Trim()
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEventResults),
            new { eventId = enrolment.EventId },
            new
            {
                result.Id,
                result.EnrolmentId,
                result.FinishTime,
                result.Position,
                result.ResultStatus
            });
    }

    // PUT: api/Results/5
    // Organisers can update results for their own events.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id,
        ResultRequest request)
    {
        var error = ValidateResult(request);

        if (error != null)
        {
            return BadRequest(error);
        }

        if (!TryGetUserId(out int organiserId))
        {
            return Unauthorized();
        }

        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (result == null)
        {
            return NotFound("Result not found.");
        }

        if (result.Enrolment?.Event == null ||
            result.Enrolment.Event.OrganiserId != organiserId)
        {
            return Forbid();
        }

        result.FinishTime = request.FinishTime;
        result.Position = request.Position;
        result.ResultStatus = request.ResultStatus.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Result updated successfully.",
            result.Id,
            result.EnrolmentId,
            result.FinishTime,
            result.Position,
            result.ResultStatus
        });
    }

    private static string? ValidateResult(ResultRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResultStatus))
        {
            return "Result status is required.";
        }

        var status = request.ResultStatus.Trim();

        if (!status.Equals("Finished", StringComparison.OrdinalIgnoreCase) &&
            !status.Equals("DNF", StringComparison.OrdinalIgnoreCase) &&
            !status.Equals("DNS", StringComparison.OrdinalIgnoreCase))
        {
            return "Result status must be Finished, DNF, or DNS.";
        }

        if (status.Equals("Finished", StringComparison.OrdinalIgnoreCase))
        {
            if (request.FinishTime <= TimeSpan.Zero)
            {
                return "Finish time must be greater than zero.";
            }

            if (request.Position <= 0)
            {
                return "Position must be greater than zero.";
            }
        }
        else if (request.Position < 0)
        {
            return "Position cannot be negative.";
        }

        return null;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId);
    }
}

// Data accepted when recording or updating a result.
public class ResultRequest
{
    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }

    public string ResultStatus { get; set; } = "Finished";
}