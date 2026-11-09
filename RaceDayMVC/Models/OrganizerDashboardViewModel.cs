namespace RaceDayMVC.Models
{
    public class OrganizerDashboardViewModel
    {
        public OrganizerProfileViewModel Profile { get; set; } = new();

        public List<EventViewModel> Events { get; set; } = new();

        public int TotalEvents => Events.Count;

        public int UpcomingEvents =>
            Events.Count(e => e.EventDate >= DateTime.Now);

        public int CompletedEvents =>
            Events.Count(e => e.EventDate < DateTime.Now);
    }
}
