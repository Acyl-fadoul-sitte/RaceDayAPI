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
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/Categories
    // Both organisers and participants can view categories.
    [HttpGet]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.EventId,
                c.Name,
                c.Description,
                c.EntryFee,
                c.MaxParticipants
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/Categories/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.EventId,
                c.Name,
                c.Description,
                c.EntryFee,
                c.MaxParticipants
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        return Ok(category);
    }

    // POST: api/Categories
    // Organisers can create categories for their own events.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        CategoryRequest request)
    {
        var error = ValidateCategory(request);

        if (error != null)
        {
            return BadRequest(error);
        }

        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int organiserId))
        {
            return Unauthorized();
        }

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId);

        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        if (eventItem.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var category = new Category
        {
            EventId = request.EventId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            EntryFee = request.EntryFee,
            MaxParticipants = request.MaxParticipants
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            new
            {
                category.Id,
                category.EventId,
                category.Name,
                category.Description,
                category.EntryFee,
                category.MaxParticipants
            });
    }

    // PUT: api/Categories/5
    // Organisers can update categories for their own events.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        CategoryRequest request)
    {
        var error = ValidateCategory(request);

        if (error != null)
        {
            return BadRequest(error);
        }

        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int organiserId))
        {
            return Unauthorized();
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        if (category.Event == null ||
            category.Event.OrganiserId != organiserId)
        {
            return Forbid();
        }

        // Keep the category attached to its original event.
        if (category.EventId != request.EventId)
        {
            return BadRequest(
                "A category cannot be moved to another event.");
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description.Trim();
        category.EntryFee = request.EntryFee;
        category.MaxParticipants = request.MaxParticipants;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Category updated successfully.",
            category.Id,
            category.EventId,
            category.Name,
            category.Description,
            category.EntryFee,
            category.MaxParticipants
        });
    }

    // DELETE: api/Categories/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int organiserId))
        {
            return Unauthorized();
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        if (category.Event == null ||
            category.Event.OrganiserId != organiserId)
        {
            return Forbid();
        }

        // The database may prevent deletion if enrolments
        // already reference this category.
        var hasEnrolments = await _context.Enrolments
            .AnyAsync(e => e.CategoryId == id);

        if (hasEnrolments)
        {
            return Conflict(
                "This category cannot be deleted because enrolments already exist.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Category deleted successfully."
        });
    }

    private static string? ValidateCategory(
        CategoryRequest request)
    {
        if (request.EventId <= 0)
        {
            return "A valid event ID is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Category name is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "Category description is required.";
        }

        if (request.EntryFee < 0)
        {
            return "Entry fee cannot be negative.";
        }

        if (request.MaxParticipants <= 0)
        {
            return "Maximum participants must be greater than zero.";
        }

        return null;
    }
}

// Data accepted when creating or updating a category.
public class CategoryRequest
{
    public int EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal EntryFee { get; set; }

    public int MaxParticipants { get; set; }
}