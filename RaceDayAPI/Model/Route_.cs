
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Model
{
    public class Route_
    {
        [Key]
        public int RouteID { get; set; }

        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Difficulty { get; set; } = string.Empty;

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event Event { get; set; } = null!;
    }
}


