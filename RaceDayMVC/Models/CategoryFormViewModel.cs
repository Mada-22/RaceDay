using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CategoryFormViewModel
    {
        public int EventID { get; set; }

        public int CategoryID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0, 120)]
        public int? MinimumAge { get; set; }

        [Range(0, 120)]
        public int? MaximumAge { get; set; }

        [Range(0.01, 1000)]
        public decimal? Distance { get; set; }

        [Range(0, 100000)]
        public decimal EntryFee { get; set; }
    }
}
