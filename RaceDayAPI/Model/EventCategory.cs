namespace RaceDayAPI.Model
{
    public class EventCategory
    {
        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public Event Event { get; set; } = null!;

        public Category Category { get; set; } = null!;
    }
    }

