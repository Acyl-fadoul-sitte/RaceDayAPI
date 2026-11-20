




using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMVC1.Models;

namespace RaceDayMVC1.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        var response = await client.PostAsJsonAsync(
            "api/Auth/login", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result == null ||
            string.IsNullOrWhiteSpace(result.Token) ||
            result.User == null)
        {
            ModelState.AddModelError("", "Login failed. Please try again.");
            return View(model);
        }

        HttpContext.Session.SetString("JWToken", result.Token);
        HttpContext.Session.SetString("UserRole", result.User.Role);
        HttpContext.Session.SetString("UserEmail", result.User.Email);
        HttpContext.Session.SetString("UserId", result.User.Id.ToString());

        if (result.User.Role == "Organiser")
        {
            return RedirectToAction("Index", "Organiser");
        }

        return RedirectToAction("Index", "Participant");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        var response = await client.PostAsJsonAsync(
            "api/Auth/register", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Registration failed. Please check your details.");
            return View(model);
        }

        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public LoginUser User { get; set; } = new();
}

public class LoginUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
