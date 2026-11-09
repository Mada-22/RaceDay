
namespace RaceDayMVC.Models
{
    public class DashboardViewModel
    {
       
        public ParticipantProfileViewModel Profile { get; set; }
            = new();

        public List<EventViewModel> UpcomingEvents { get; set; }
            = new();
        public List<EntryViewModel> MyEntries { get; set; } = new();

        public int RegistrationCount => MyEntries.Count;

        public List<ResultViewModel> MyResults { get; set; } = new();

        public int CompletedRaceCount => MyResults.Count;
    }
}
