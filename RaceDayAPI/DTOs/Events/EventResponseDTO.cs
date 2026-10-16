namespace RaceDayAPI.DTOs.Events
{
    public class EventResponseDTO
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal Distance { get; set; }

        public string EventType { get; set; } = string.Empty;

        public DateTime OpenDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public int OrganizerID { get; set; }

        public string OrganizationName { get; set; } = string.Empty;
    }
}
