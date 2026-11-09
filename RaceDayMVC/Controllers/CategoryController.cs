using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class CategoryController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public CategoryController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            // Retrieve the event.
            using var eventRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Event/{eventId}");

            var eventResponse = await _apiClient.SendAsync(eventRequest);

            if (!eventResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve the event.";
                return RedirectToAction("Index", "Organizer");
            }

            var eventItem = await eventResponse.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
            {
                return NotFound();
            }

            // Retrieve the organizer's profile.
            using var profileRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Organizer/profile");

            var profileResponse = await _apiClient.SendAsync(profileRequest);

            if (!profileResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to verify your organizer profile.";
                return RedirectToAction("Index", "Organizer");
            }

            var profile = await profileResponse.Content
                .ReadFromJsonAsync<OrganizerProfileViewModel>();

            if (profile == null || eventItem.OrganizerID != profile.OrganizerID)
            {
                return Forbid();
            }

            // Retrieve categories belonging to this event.
            using var categoryRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Category/event/{eventId}");

            var categoryResponse = await _apiClient.SendAsync(categoryRequest);

            if (!categoryResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve event categories.";
                return RedirectToAction("Index", "Organizer");
            }

            var categories = await categoryResponse.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            var viewModel = new CategoryManagementViewModel
            {
                EventID = eventItem.EventID,
                EventName = eventItem.EventName,
                Categories = categories ?? new List<CategoryViewModel>()
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Event/{eventId}");

            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Organizer");
            }

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
            {
                return NotFound();
            }

            using var profileRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Organizer/profile");

            var profileResponse = await _apiClient.SendAsync(profileRequest);

            if (!profileResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Organizer");
            }

            var profile = await profileResponse.Content
                .ReadFromJsonAsync<OrganizerProfileViewModel>();

            if (profile == null || eventItem.OrganizerID != profile.OrganizerID)
            {
                return Forbid();
            }

            var model = new CategoryFormViewModel
            {
                EventID = eventId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categoryData = new
            {
                model.CategoryName,
                model.MinimumAge,
                model.MaximumAge,
                model.Distance,
                model.EntryFee
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/Category/event/{model.EventID}")
            {
                Content = JsonContent.Create(categoryData)
            };

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category created successfully.";

                return RedirectToAction(
                    "Index",
                    new { eventId = model.EventID });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to create the category."
                    : errorMessage);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int eventId, int categoryId)
        {
            // Retrieve event
            using var eventRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Event/{eventId}");

            var eventResponse = await _apiClient.SendAsync(eventRequest);

            if (!eventResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Organizer");
            }

            var eventItem = await eventResponse.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
            {
                return NotFound();
            }

            // Verify organizer ownership
            using var profileRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Organizer/profile");

            var profileResponse = await _apiClient.SendAsync(profileRequest);

            if (!profileResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Organizer");
            }

            var profile = await profileResponse.Content
                .ReadFromJsonAsync<OrganizerProfileViewModel>();

            if (profile == null || eventItem.OrganizerID != profile.OrganizerID)
            {
                return Forbid();
            }

            // Retrieve categories for this event
            using var categoryRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Category/event/{eventId}");

            var categoryResponse = await _apiClient.SendAsync(categoryRequest);

            if (!categoryResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { eventId });
            }

            var categories = await categoryResponse.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            var category = categories?.FirstOrDefault(
                c => c.CategoryID == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryFormViewModel
            {
                EventID = eventId,
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                MinimumAge = category.MinimumAge,
                MaximumAge = category.MaximumAge,
                Distance = category.Distance,
                EntryFee = category.EntryFee
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categoryData = new
            {
                model.CategoryName,
                model.MinimumAge,
                model.MaximumAge,
                model.Distance,
                model.EntryFee
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/Category/event/{model.EventID}/category/{model.CategoryID}")
            {
                Content = JsonContent.Create(categoryData)
            };

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category updated successfully.";

                return RedirectToAction(
                    "Index",
                    new { eventId = model.EventID });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to update the category."
                    : errorMessage);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int eventId, int categoryId)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/Category/event/{eventId}/category/{categoryId}");

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to delete the category."
                    : errorMessage;
            }

            return RedirectToAction("Index", new { eventId });
        }
    }
}
