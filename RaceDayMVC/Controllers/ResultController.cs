using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ResultController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public ResultController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Result/my-results");

            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve your results.";

                return View(new List<ResultViewModel>());
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<ResultViewModel>>();

            return View(results ?? new List<ResultViewModel>());
        }
    }
}
