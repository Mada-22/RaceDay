using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;
        [Range(0.01, 1000)]
        public decimal Distance { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "Event type must be Run, Walk or Cycle.")]
        public string EventType { get; set; } = string.Empty;

        public DateTime OpenDate { get; set; }

        public DateTime ClosingDate { get; set; }
    }
}