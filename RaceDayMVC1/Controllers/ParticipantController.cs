









using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMVC1.Models;

namespace RaceDayMVC1.Controllers;

public class ParticipantController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ParticipantController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Participant Dashboard
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("UserRole") != "Participant")
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/Events");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage =
                "Unable to load events. Please try again.";

            return View(new List<EventViewModel>());
        }

        var events = await response.Content
            .ReadFromJsonAsync<List<EventViewModel>>();

        return View(events ?? new List<EventViewModel>());
    }

    // Event Details and Categories
    public async Task<IActionResult> Details(int id)
    {
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("UserRole") != "Participant")
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var eventResponse = await client.GetAsync($"api/Events/{id}");

        if (!eventResponse.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var raceEvent = await eventResponse.Content
            .ReadFromJsonAsync<EventViewModel>();

        if (raceEvent == null)
            return RedirectToAction("Index");

        var categoryResponse = await client.GetAsync("api/Categories");

        if (!categoryResponse.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage =
                "Unable to load event categories.";

            return View(raceEvent);
        }

        var categories = await categoryResponse.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>();

        ViewBag.Categories =
            (categories ?? new List<CategoryViewModel>())
            .Where(c => c.EventId == id)
            .ToList();

        return View(raceEvent);
    }

    // My Enrolments
    public async Task<IActionResult> MyEnrolments()
    {
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("UserRole") != "Participant")
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/Enrolments/my");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage =
                "Unable to load your enrolments. Please try again.";

            return View(new List<EnrolmentViewModel>());
        }

        var enrolments = await response.Content
            .ReadFromJsonAsync<List<EnrolmentViewModel>>();

        return View(enrolments ?? new List<EnrolmentViewModel>());
    }

    // My Race Results
    public async Task<IActionResult> MyResults()
    {
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("UserRole") != "Participant")
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/Results/my");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage =
                "Unable to load your race results. Please try again.";

            return View(new List<ResultViewModel>());
        }

        var results = await response.Content
            .ReadFromJsonAsync<List<ResultViewModel>>();

        return View(results ?? new List<ResultViewModel>());
    }

    // Enrol in an Event
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(
        EnrolmentRequestViewModel model)
    {
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("UserRole") != "Participant")
            return RedirectToAction("Login", "Auth");

        if (model.EventId <= 0 || model.CategoryId <= 0)
        {
            TempData["EnrolmentMessage"] =
                "Please select a valid event category.";

            return RedirectToAction(
                "Details",
                new { id = model.EventId });
        }

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            "api/Enrolments",
            model);

        if (response.IsSuccessStatusCode)
        {
            TempData["EnrolmentMessage"] =
                "You have successfully enrolled in this event!";

            return RedirectToAction("MyEnrolments");
        }

        var errorMessage =
            await response.Content.ReadAsStringAsync();

        TempData["EnrolmentMessage"] =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "Enrolment failed. Please try again."
                : errorMessage;

        return RedirectToAction(
            "Details",
            new { id = model.EventId });
    }

    // Cancel an enrolment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("UserRole") != "Participant")
            return RedirectToAction("Login", "Auth");

        if (id <= 0)
        {
            TempData["EnrolmentMessage"] =
                "Invalid enrolment ID.";

            return RedirectToAction("MyEnrolments");
        }

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync(
            $"api/Enrolments/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["EnrolmentMessage"] =
                "Your enrolment has been cancelled successfully.";
        }
        else
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync();

            TempData["EnrolmentMessage"] =
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to cancel your enrolment. Please try again."
                    : errorMessage;
        }

        return RedirectToAction("MyEnrolments");
    }
}
