using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Events
{
    public class UpdateEventDTO
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

        [Range(typeof(decimal), "0.01", "1000")]
        public decimal Distance { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$")]
        public string EventType { get; set; } = string.Empty;

        public DateTime OpenDate { get; set; }

        public DateTime ClosingDate { get; set; }
    }
}
