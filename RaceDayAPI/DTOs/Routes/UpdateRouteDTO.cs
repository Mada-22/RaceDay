using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Routes
{
    public class UpdateRouteDTO
    {
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Difficulty { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "1000")]
        public decimal Distance { get; set; }
    }
}
