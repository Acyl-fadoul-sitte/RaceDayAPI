







using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC1.Models;

namespace RaceDayMVC1.Controllers;

public class OrganiserController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrganiserController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        var token = HttpContext.Session.GetString("JWToken");

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private bool IsOrganiserLoggedIn()
    {
        var token = HttpContext.Session.GetString("JWToken");
        var role = HttpContext.Session.GetString("UserRole");

        return !string.IsNullOrEmpty(token) && role == "Organiser";
    }

    private int GetOrganiserId()
    {
        return int.TryParse(
            HttpContext.Session.GetString("UserId"),
            out var id) ? id : 0;
    }

    public IActionResult Index()
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        return View();
    }

    public async Task<IActionResult> MyEvents()
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();
        var response = await client.GetAsync("api/Events");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load your events.";
            return View(new List<EventViewModel>());
        }

        var events = await response.Content
            .ReadFromJsonAsync<List<EventViewModel>>()
            ?? new List<EventViewModel>();

        var organiserId = GetOrganiserId();

        if (organiserId == 0)
            return RedirectToAction("Login", "Auth");

        var myEvents = events
            .Where(e => e.OrganiserId == organiserId)
            .OrderBy(e => e.EventDate)
            .ToList();

        return View(myEvents);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        return View(new CreateEventViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        if (model.EventDate.Date <= DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(model.EventDate),
                "Please select a future event date.");

            return View(model);
        }

        var client = CreateApiClient();

        var eventRequest = new
        {
            model.Name,
            model.Description,
            model.EventDate,
            model.Location,
            model.EventType,
            model.DistanceKm
        };

        var response = await client.PostAsJsonAsync(
            "api/Events", eventRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                string.IsNullOrWhiteSpace(error)
                    ? "Could not create the event."
                    : error);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Your event was created successfully.";

        return RedirectToAction(nameof(MyEvents));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();
        var response = await client.GetAsync($"api/Events/{id}");

        if (!response.IsSuccessStatusCode)
            return NotFound("Event not found.");

        var eventItem = await response.Content
            .ReadFromJsonAsync<EventViewModel>();

        var organiserId = GetOrganiserId();

        if (eventItem == null ||
            organiserId == 0 ||
            eventItem.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var model = new CreateEventViewModel
        {
            Name = eventItem.Name,
            Description = eventItem.Description,
            EventDate = eventItem.EventDate,
            Location = eventItem.Location,
            EventType = eventItem.EventType,
            DistanceKm = eventItem.DistanceKm
        };

        ViewBag.EventId = id;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(
        int id, CreateEventViewModel model)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        ViewBag.EventId = id;

        if (!ModelState.IsValid)
            return View(model);

        if (model.EventDate.Date <= DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(model.EventDate),
                "Please select a future event date.");

            return View(model);
        }

        var client = CreateApiClient();

        var eventRequest = new
        {
            model.Name,
            model.Description,
            model.EventDate,
            model.Location,
            model.EventType,
            model.DistanceKm
        };

        var response = await client.PutAsJsonAsync(
            $"api/Events/{id}", eventRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                string.IsNullOrWhiteSpace(error)
                    ? "Could not update the event."
                    : error);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Your event was updated successfully.";

        return RedirectToAction(nameof(MyEvents));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();
        var eventResponse = await client.GetAsync($"api/Events/{id}");

        if (!eventResponse.IsSuccessStatusCode)
            return NotFound("Event not found.");

        var eventItem = await eventResponse.Content
            .ReadFromJsonAsync<EventViewModel>();

        var organiserId = GetOrganiserId();

        if (eventItem == null ||
            organiserId == 0 ||
            eventItem.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var response = await client.DeleteAsync($"api/Events/{id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Could not delete the event.";
            return RedirectToAction(nameof(MyEvents));
        }

        TempData["SuccessMessage"] =
            "Your event was deleted successfully.";

        return RedirectToAction(nameof(MyEvents));
    }


    // GET: Organiser/ManageCategories
    [HttpGet]
    public async Task<IActionResult> ManageCategories()
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var eventResponse = await client.GetAsync("api/Events");

        if (!eventResponse.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load your events.";
            return View(new List<EventCategoriesViewModel>());
        }

        var events = await eventResponse.Content
            .ReadFromJsonAsync<List<EventViewModel>>()
            ?? new List<EventViewModel>();

        var organiserId = GetOrganiserId();

        if (organiserId == 0)
            return RedirectToAction("Login", "Auth");

        var myEvents = events
            .Where(e => e.OrganiserId == organiserId)
            .OrderBy(e => e.Name)
            .ToList();

        var categoryResponse = await client.GetAsync("api/Categories");

        if (!categoryResponse.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load categories.";
            return View(new List<EventCategoriesViewModel>());
        }

        var categories = await categoryResponse.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>()
            ?? new List<CategoryViewModel>();

        var result = myEvents.Select(e => new EventCategoriesViewModel
        {
            Event = e,
            Categories = categories
                .Where(c => c.EventId == e.Id)
                .ToList()
        }).ToList();

        return View(result);
    }


    // GET: Organiser/CreateCategory
    [HttpGet]
    public async Task<IActionResult> CreateCategory(int eventId)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var response = await client.GetAsync($"api/Events/{eventId}");

        if (!response.IsSuccessStatusCode)
            return NotFound("Event not found.");

        var eventItem = await response.Content
            .ReadFromJsonAsync<EventViewModel>();

        if (eventItem == null ||
            eventItem.OrganiserId != GetOrganiserId())
            return Forbid();

        var model = new CategoryViewModel
        {
            EventId = eventId
        };

        ViewBag.EventName = eventItem.Name;

        return View(model);
    }

    // POST: Organiser/CreateCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(
        CategoryViewModel model)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var client = CreateApiClient();

        var request = new
        {
            model.EventId,
            model.Name,
            model.Description,
            model.EntryFee,
            MaxParticipants = model.MaxParticipants
        };

        var response = await client.PostAsJsonAsync(
            "api/Categories", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                string.IsNullOrWhiteSpace(error)
                    ? "Could not create category."
                    : error);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Category created successfully.";

        return RedirectToAction(nameof(ManageCategories));
    }

    // GET: Organiser/EditCategory
    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var response = await client.GetAsync($"api/Categories/{id}");

        if (!response.IsSuccessStatusCode)
            return NotFound("Category not found.");

        var model = await response.Content
            .ReadFromJsonAsync<CategoryViewModel>();

        if (model == null)
            return NotFound();

        var eventResponse = await client.GetAsync(
            $"api/Events/{model.EventId}");

        if (!eventResponse.IsSuccessStatusCode)
            return NotFound("Event not found.");

        var eventItem = await eventResponse.Content
            .ReadFromJsonAsync<EventViewModel>();

        if (eventItem == null ||
            eventItem.OrganiserId != GetOrganiserId())
            return Forbid();

        ViewBag.EventName = eventItem.Name;

        return View(model);
    }

    // POST: Organiser/EditCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(
        int id, CategoryViewModel model)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var client = CreateApiClient();

        var request = new
        {
            model.EventId,
            model.Name,
            model.Description,
            model.EntryFee,
            MaxParticipants = model.MaxParticipants
        };

        var response = await client.PutAsJsonAsync(
            $"api/Categories/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                string.IsNullOrWhiteSpace(error)
                    ? "Could not update category."
                    : error);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Category updated successfully.";

        return RedirectToAction(nameof(ManageCategories));
    }

    // POST: Organiser/DeleteCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var response = await client.DeleteAsync(
            $"api/Categories/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                string.IsNullOrWhiteSpace(error)
                    ? "Could not delete category."
                    : error;

            return RedirectToAction(nameof(ManageCategories));
        }

        TempData["SuccessMessage"] =
            "Category deleted successfully.";

        return RedirectToAction(nameof(ManageCategories));
    }



    // GET: Organiser/EventEnrolments
    [HttpGet]
    public async Task<IActionResult> EventEnrolments(int eventId)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        // Get event details
        var eventResponse = await client.GetAsync(
            $"api/Events/{eventId}");

        if (!eventResponse.IsSuccessStatusCode)
            return NotFound("Event not found.");

        var eventItem = await eventResponse.Content
            .ReadFromJsonAsync<EventViewModel>();

        var organiserId = GetOrganiserId();

        // Check event ownership
        if (eventItem == null ||
            organiserId == 0 ||
            eventItem.OrganiserId != organiserId)
        {
            return Forbid();
        }

        // Get registered participants
        var response = await client.GetAsync(
            $"api/Enrolments/event/{eventId}");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load event enrolments.";
            ViewBag.EventName = eventItem.Name;

            return View(new List<EnrolmentViewModel>());
        }

        var enrolments = await response.Content
            .ReadFromJsonAsync<List<EnrolmentViewModel>>()
            ?? new List<EnrolmentViewModel>();

        ViewBag.EventName = eventItem.Name;
        ViewBag.EventId = eventId;

        return View(enrolments);
    }
}
