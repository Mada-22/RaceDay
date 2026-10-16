using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Model
{
    public class Entry
    {
        [Key]
        public int EntryID { get; set; }

        public int EventID { get; set; }

        public int ParticipantID { get; set; }

        public int CategoryID { get; set; }

        public int? RouteID { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(ParticipantID))]
        public Participant Participant { get; set; } = null!;

        [ForeignKey(nameof(CategoryID))]
        public Category Category { get; set; } = null!;

        [ForeignKey(nameof(RouteID))]
        public Route_? Route { get; set; }

        public Result? Result { get; set; }
    }
}


