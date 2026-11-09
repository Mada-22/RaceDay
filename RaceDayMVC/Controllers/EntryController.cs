
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    [Authorize(Roles = "Participant")]
    public class EntryController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public EntryController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(EntryRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please select a valid category.";
                return RedirectToAction("Details", "Event",
                    new { id = model.EventID });
            }

            var registrationRequest = new
            {
                EventID = model.EventID,
                CategoryID = model.CategoryID,
                RouteID = model.RouteID
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Entry")
            {
                Content = JsonContent.Create(registrationRequest)
            };

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "You have successfully registered for this event.";

                return RedirectToAction("Details", "Event",
                    new { id = model.EventID });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Registration failed. Please try again."
                    : errorMessage;

            return RedirectToAction("Details", "Event",
                new { id = model.EventID });
        }
        [HttpGet]
        public async Task<IActionResult> MyEntries()
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Entry/my-entries");

            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve your registrations.";

                return View(new List<EntryViewModel>());
            }

            var entries = await response.Content
                .ReadFromJsonAsync<List<EntryViewModel>>();

            return View(entries ?? new List<EntryViewModel>());
        }

        [Authorize(Roles = "Organizer")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Event/{id}");

            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve the event.";
                return RedirectToAction("Index", "Organizer");
            }

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
            {
                return NotFound();
            }

            var model = new EditEventViewModel
            {
                EventID = eventItem.EventID,
                EventName = eventItem.EventName,
                Description = eventItem.Description,
                EventDate = eventItem.EventDate,
                Location = eventItem.Location,
                Distance = eventItem.Distance,
                EventType = eventItem.EventType,
                OpenDate = eventItem.OpenDate,
                ClosingDate = eventItem.ClosingDate
            };

            return View(model);
        }

        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updateData = new
            {
                model.EventName,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.EventType,
                model.OpenDate,
                model.ClosingDate
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/Event/{model.EventID}")
            {
                Content = JsonContent.Create(updateData)
            };

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event updated successfully.";
                return RedirectToAction("Index", "Organizer");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to update the event. Please try again."
                    : errorMessage);

            return View(model);
        }
    }
}


