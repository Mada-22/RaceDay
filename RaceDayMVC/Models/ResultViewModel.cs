namespace RaceDayMVC.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EntryID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int ParticipantID { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public TimeSpan TimeCompleted { get; set; }

        public int FinishingPosition { get; set; }
    }
}
