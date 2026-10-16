using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Categories
{
    public class UpdateCategoryDTO
    {
        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0, 120)]
        public int? MinimumAge { get; set; }

        [Range(0, 120)]
        public int? MaximumAge { get; set; }

        [Range(typeof(decimal), "0.01", "1000")]
        public decimal? Distance { get; set; }
    }
}
