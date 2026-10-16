namespace RaceDayAPI.Model
{
    public class Participant
    {
        public int ParticipantID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;

        public int Age { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public int AccountID { get; set; }

        public Account? Account { get; set; }

        public ICollection<Entry> Entries { get; set; }
            = new List<Entry>();
    }
}
