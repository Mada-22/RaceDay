using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public EventController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Event");

            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to retrieve events.";

                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }



        public async Task<IActionResult> Details(int id)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Event/{id}");

            var response = await _apiClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to retrieve event details.";
                return RedirectToAction(nameof(Index));
            }

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
            {
                return NotFound();
            }
            // Retrieve categories belonging to this event
            using var categoriesRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Category/event/{id}");

            var categoriesResponse = await _apiClient.SendAsync(categoriesRequest);

            if (categoriesResponse.IsSuccessStatusCode)
            {
                var categories = await categoriesResponse.Content
                    .ReadFromJsonAsync<List<CategoryViewModel>>();

                eventItem.Categories = categories ?? new List<CategoryViewModel>();
            }

            return View(eventItem);
        }
        [Authorize(Roles = "Organizer")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateEventViewModel());
        }

        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Event")
            {
                Content = JsonContent.Create(model)
            };

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event created successfully.";

                return RedirectToAction("Index", "Organizer");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to create the event. Please try again."
                    : errorMessage);

            return View(model);
        }
        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/Event/{id}");

            var response = await _apiClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event deleted successfully.";
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to delete the event."
                    : errorMessage;
            }

            return RedirectToAction("Index", "Organizer");
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
    }

}