
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Model
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime OpenDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ClosingDate { get; set; }

        [Required]
        [StringLength(20)]
        public string EventType { get; set; } = string.Empty;
       
        [Required]
        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }
        public int OrganizerID { get; set; }

        [ForeignKey(nameof(OrganizerID))]
        public Organizer Organizer { get; set; } = null!;

        public List<Route_> Routes { get; set; } = new();

        public List<Entry> Entries { get; set; } = new();

        public List<EventCategory> EventCategories { get; set; } = new();
    }
}


 