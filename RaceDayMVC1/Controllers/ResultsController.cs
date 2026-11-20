

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC1.Models;

namespace RaceDayMVC1.Controllers;

public class ResultsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResultsController(IHttpClientFactory httpClientFactory)
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
        return !string.IsNullOrEmpty(
                   HttpContext.Session.GetString("JWToken"))
               && HttpContext.Session.GetString("UserRole") == "Organiser";
    }

    private bool IsParticipantLoggedIn()
    {
        return !string.IsNullOrEmpty(
                   HttpContext.Session.GetString("JWToken"))
               && HttpContext.Session.GetString("UserRole") == "Participant";
    }

    // GET: Results/MyResults
    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipantLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var response = await client.GetAsync("api/Results/my");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load your results.";
            return View(new List<ResultViewModel>());
        }

        var results = await response.Content
            .ReadFromJsonAsync<List<ResultViewModel>>()
            ?? new List<ResultViewModel>();

        return View(results);
    }

    // GET: Results/EventResults/5
    public async Task<IActionResult> EventResults(int eventId)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var response = await client.GetAsync(
            $"api/Results/event/{eventId}");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load event results.";
            return View(new List<ResultViewModel>());
        }

        var results = await response.Content
            .ReadFromJsonAsync<List<ResultViewModel>>()
            ?? new List<ResultViewModel>();

        ViewBag.EventId = eventId;

        return View(results);
    }

    // GET: Results/Create/5
    public IActionResult Create(int enrolmentId, int eventId)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var model = new ResultViewModel
        {
            EnrolmentId = enrolmentId,
            EventId = eventId,
            ResultStatus = "Finished"
        };

        return View(model);
    }

    // POST: Results/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResultViewModel model)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var client = CreateApiClient();

        var request = new
        {
            FinishTime = model.FinishTime,
            Position = model.Position,
            ResultStatus = model.ResultStatus
        };

        var response = await client.PostAsJsonAsync(
            $"api/Results/enrolment/{model.EnrolmentId}",
            request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Could not save the result. Please check the details.");

            return View(model);
        }

        return RedirectToAction(
            nameof(EventResults),
            new { eventId = model.EventId });
    }

    // GET: Results/Edit/5
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        var client = CreateApiClient();

        var response = await client.GetAsync(
            $"api/Results/event/{eventId}");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var results = await response.Content
            .ReadFromJsonAsync<List<ResultViewModel>>()
            ?? new List<ResultViewModel>();

        var result = results.FirstOrDefault(r => r.Id == id);

        if (result == null)
            return NotFound();

        result.EventId = eventId;

        return View(result);
    }

    // POST: Results/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ResultViewModel model)
    {
        if (!IsOrganiserLoggedIn())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var client = CreateApiClient();

        var request = new
        {
            FinishTime = model.FinishTime,
            Position = model.Position,
            ResultStatus = model.ResultStatus
        };

        var response = await client.PutAsJsonAsync(
            $"api/Results/{model.Id}",
            request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Could not update the result.");

            return View(model);
        }

        return RedirectToAction(
            nameof(EventResults),
            new { eventId = model.EventId });
    }
}
