namespace RaceDayAPI.DTOs.Routes
{
    public class RouteResponseDTO
    {
        public int RouteID { get; set; }

        public int EventID { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public string Difficulty { get; set; } = string.Empty;

        public decimal Distance { get; set; }
    }
}
