using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Model
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        public int? MinimumAge { get; set; }

        public int? MaximumAge { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? Distance { get; set; }

        public List<EventCategory> EventCategories { get; set; } = new();

        public List<Result> Results { get; set; } = new();
    }
}


