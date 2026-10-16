namespace RaceDayAPI.Model
{
    public class Organizer
    {
        public int OrganizerID { get; set; }

        public string OrganizationName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int AccountID { get; set; }

        public Account? Account { get; set; }

        public ICollection<Event> Events { get; set; }
            = new List<Event>();
    }
}
