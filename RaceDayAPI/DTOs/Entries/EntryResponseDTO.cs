namespace RaceDayAPI.DTOs.Entries
{
    public class EntryResponseDTO
    {
        public int EntryID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int ParticipantID { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int? RouteID { get; set; }

        public string? RouteName { get; set; }

        public decimal Price { get; set; }
    }
}
