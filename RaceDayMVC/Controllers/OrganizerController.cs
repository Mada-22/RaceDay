using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public OrganizerController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new OrganizerDashboardViewModel();

            // Retrieve organizer profile
            using var profileRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Organizer/profile");

            var profileResponse = await _apiClient.SendAsync(profileRequest);

            if (!profileResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve your organizer profile.";

                return View(viewModel);
            }

            var profile = await profileResponse.Content
                .ReadFromJsonAsync<OrganizerProfileViewModel>();

            if (profile == null)
            {
                TempData["ErrorMessage"] =
                    "Your organizer profile could not be found.";

                return View(viewModel);
            }

            // Retrieve events
            using var eventsRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Event");

            var eventsResponse = await _apiClient.SendAsync(eventsRequest);

            if (!eventsResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve your events.";

                viewModel.Profile = profile;

                return View(viewModel);
            }

            var allEvents = await eventsResponse.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            // Only display events belonging to this organizer
            viewModel.Profile = profile;

            viewModel.Events = (allEvents ?? new List<EventViewModel>())
                .Where(e => e.OrganizerID == profile.OrganizerID)
                .OrderBy(e => e.EventDate)
                .ToList();

            return View(viewModel);
        }
    }
}
