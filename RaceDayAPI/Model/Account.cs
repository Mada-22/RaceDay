namespace RaceDayAPI.Model
{
    public class Account
    {
        public int AccountID { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string AccountType { get; set; } = string.Empty;

        public Participant? Participant { get; set; }

        public Organizer? Organizer { get; set; }
    }
}
