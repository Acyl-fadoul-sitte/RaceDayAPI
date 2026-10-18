


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
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/Events
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.EventType,
                e.DistanceKm,
                e.OrganiserId
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/Events/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventItem = await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.EventType,
                e.DistanceKm,
                e.OrganiserId
            })
            .FirstOrDefaultAsync();

        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        return Ok(eventItem);
    }

    // POST: api/Events
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(
                "Name, description, and location are required.");
        }

        if (request.DistanceKm <= 0)
        {
            return BadRequest(
                "Distance must be greater than zero.");
        }

        if (request.EventDate == default ||
            request.EventDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(
                "Please provide a valid event date that is not in the past.");
        }

        if (!IsValidEventType(request.EventType))
        {
            return BadRequest(
                "Event type must be Running, Walking, or Cycling.");
        }

        if (!TryGetUserId(out var organiserId))
        {
            return Forbid();
        }

        var organiserExists = await _context.Users
            .AnyAsync(u => u.Id == organiserId);

        if (!organiserExists)
        {
            return BadRequest(
                "The authenticated organiser was not found.");
        }

        var eventItem = new Event
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            EventDate = request.EventDate.Date,
            Location = request.Location.Trim(),
            EventType = NormalizeEventType(request.EventType),
            DistanceKm = request.DistanceKm,
            OrganiserId = organiserId
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = eventItem.Id },
            new
            {
                eventItem.Id,
                eventItem.Name,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.EventType,
                eventItem.DistanceKm,
                eventItem.OrganiserId
            });
    }

    // PUT: api/Events/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var eventItem = await _context.Events.FindAsync(id);

        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        if (!TryGetUserId(out var userId) ||
            eventItem.OrganiserId != userId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(
                "Name, description, and location are required.");
        }

        if (request.DistanceKm <= 0)
        {
            return BadRequest(
                "Distance must be greater than zero.");
        }

        if (request.EventDate == default ||
            request.EventDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(
                "Please provide a valid event date that is not in the past.");
        }

        if (!IsValidEventType(request.EventType))
        {
            return BadRequest(
                "Event type must be Running, Walking, or Cycling.");
        }

        eventItem.Name = request.Name.Trim();
        eventItem.Description = request.Description.Trim();
        eventItem.EventDate = request.EventDate.Date;
        eventItem.Location = request.Location.Trim();
        eventItem.EventType = NormalizeEventType(request.EventType);
        eventItem.DistanceKm = request.DistanceKm;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully.",
            eventItem.Id,
            eventItem.Name,
            eventItem.Description,
            eventItem.EventDate,
            eventItem.Location,
            eventItem.EventType,
            eventItem.DistanceKm,
            eventItem.OrganiserId
        });
    }

    // DELETE: api/Events/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.Events.FindAsync(id);

        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        if (!TryGetUserId(out var userId) ||
            eventItem.OrganiserId != userId)
        {
            return Forbid();
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event deleted successfully."
        });
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;

        return int.TryParse(userIdValue, out userId);
    }

    private static bool IsValidEventType(string? eventType)
    {
        return !string.IsNullOrWhiteSpace(eventType)
            && (
                eventType.Equals(
                    "Running",
                    StringComparison.OrdinalIgnoreCase)
                || eventType.Equals(
                    "Walking",
                    StringComparison.OrdinalIgnoreCase)
                || eventType.Equals(
                    "Cycling",
                    StringComparison.OrdinalIgnoreCase)
            );
    }

    private static string NormalizeEventType(string eventType)
    {
        if (eventType.Equals(
            "Running", StringComparison.OrdinalIgnoreCase))
        {
            return "Running";
        }

        if (eventType.Equals(
            "Walking", StringComparison.OrdinalIgnoreCase))
        {
            return "Walking";
        }

        return "Cycling";
    }
}

public class EventRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }
}
