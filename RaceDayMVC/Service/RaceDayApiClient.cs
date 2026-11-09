using System.Net.Http.Json;

namespace RaceDayMVC.Services
{
    public class RaceDayApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string ApiCookieKey = "RaceDay.ApiAuthCookie";

        public RaceDayApiClient(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // Sends a request to RaceDayAPI
        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException(
                    "No active HTTP context.");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Retrieve this user's API authentication cookie.
            var apiCookie = context.Session.GetString(ApiCookieKey);

            // Forward the cookie to the API.
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);
            }

            return await client.SendAsync(request, cancellationToken);
        }

        // Logs the user into RaceDayAPI.
        public async Task<HttpResponseMessage> LoginAsync<T>(
            T loginRequest,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Account/login")
            {
                Content = JsonContent.Create(loginRequest)
            };

            var response = await SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // Capture the authentication cookie returned by the API.
                if (response.Headers.TryGetValues(
                    "Set-Cookie", out var cookies))
                {
                    var apiCookie = cookies.FirstOrDefault(cookie =>
                        cookie.StartsWith(
                            "RaceDay.Auth=",
                            StringComparison.OrdinalIgnoreCase));

                    if (apiCookie != null)
                    {
                        // Store only the cookie name and value.
                        var cookieValue = apiCookie.Split(';')[0];

                        _httpContextAccessor.HttpContext!
                            .Session.SetString(
                                ApiCookieKey, cookieValue);
                    }
                }
            }

            return response;
        }

        // Removes the stored API authentication cookie.
        public void ClearApiCookie()
        {
            _httpContextAccessor.HttpContext!
                .Session.Remove(ApiCookieKey);
        }
    }
}
