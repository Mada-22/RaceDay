using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    
    [Authorize(Roles = "Participant")]
    public class DashboardController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public DashboardController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<IActionResult> Index()
        {
            // Retrieve participant profile
            using var profileRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Participant/profile");

            var profileResponse = await _apiClient.SendAsync(profileRequest);

            if (!profileResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await profileResponse.Content
                .ReadFromJsonAsync<ParticipantProfileViewModel>();

            if (profile == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve events
            using var eventsRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Event");

            var eventsResponse = await _apiClient.SendAsync(eventsRequest);

            List<EventViewModel> upcomingEvents = new();

            if (eventsResponse.IsSuccessStatusCode)
            {
                var events = await eventsResponse.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                if (events != null)
                {
                    upcomingEvents = events
                        .Where(e => e.EventDate >= DateTime.Today)
                        .OrderBy(e => e.EventDate)
                        .Take(5)
                        .ToList();
                }
            }
            using var entriesRequest = new HttpRequestMessage(
    HttpMethod.Get,
    "api/Entry/my-entries");

            var entriesResponse = await _apiClient.SendAsync(entriesRequest);

            List<EntryViewModel> myEntries = new();

            if (entriesResponse.IsSuccessStatusCode)
            {
                myEntries = await entriesResponse.Content
                    .ReadFromJsonAsync<List<EntryViewModel>>()
                    ?? new List<EntryViewModel>();
            }
            using var resultsRequest = new HttpRequestMessage(
                 HttpMethod.Get,
                    "api/Result/my-results");

            var resultsResponse = await _apiClient.SendAsync(resultsRequest);

            List<ResultViewModel> myResults = new();

            if (resultsResponse.IsSuccessStatusCode)
            {
                myResults = await resultsResponse.Content
                    .ReadFromJsonAsync<List<ResultViewModel>>()
                    ?? new List<ResultViewModel>();
            }

            // Pass data to dashboard
            var dashboard = new DashboardViewModel
            {
                Profile = profile,
                UpcomingEvents = upcomingEvents,
                MyEntries= myEntries,
                MyResults= myResults
            };

            return View(dashboard);
        }
    }
    }

