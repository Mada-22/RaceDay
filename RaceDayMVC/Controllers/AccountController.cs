using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Net.Http.Json;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public AccountController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var response = await _apiClient.LoginAsync(model);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid username or password.");

                    return View(model);
                }

                var loginResponse =
                    await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

                if (loginResponse == null ||
                    string.IsNullOrWhiteSpace(loginResponse.AccountType))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The server returned an invalid login response.");

                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        loginResponse.AccountID.ToString()),

                    new Claim(
                        ClaimTypes.Name,
                        loginResponse.UserName),

                    new Claim(
                        ClaimTypes.Role,
                        loginResponse.AccountType)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);
                if (loginResponse.AccountType == "Participant")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                if (loginResponse.AccountType == "Organizer")
                {
                    return RedirectToAction("Index", "Organizer");
                }

                return RedirectToAction("Index", "Home");
              ;
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay server.");

                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/Account/logout");

                using var response = await _apiClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                // Continue clearing the local session even if the API is unavailable.
            }
            finally
            {
                _apiClient.ClearApiCookie();

                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }


            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegisterParticipant()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterParticipant(
     ParticipantRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registrationRequest = new
                {
                    model.UserName,
                    model.UserEmail,
                    model.Password,
                    model.Name,
                    model.Surname,
                    model.Age,
                    model.PhoneNumber
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/Account/register/participant")
                {
                    Content = JsonContent.Create(registrationRequest)
                };

                using var response = await _apiClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "Registration failed. Please check your details and try again.");

                    return View(model);
                }

                TempData["SuccessMessage"] =
                    "Your participant account has been created. Please log in.";

                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay server.");

                return View(model);
            }
        }
        [HttpGet]
        public IActionResult RegisterOrganizer()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterOrganizer(
    OrganizerRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registrationRequest = new
                {
                    model.UserName,
                    model.UserEmail,
                    model.Password,
                    model.OrganizationName,
                    model.Address,
                    model.PhoneNumber,
                    model.Email
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/Account/register/organizer")
                {
                    Content = JsonContent.Create(registrationRequest)
                };

                using var response = await _apiClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Registration failed. Please check your details and try again.");

                    return View(model);
                }

                TempData["SuccessMessage"] =
                    "Your organizer account has been created. Please log in.";

                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay server.");

                return View(model);
            }
        }
    }
}
