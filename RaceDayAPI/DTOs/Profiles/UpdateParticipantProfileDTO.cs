using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Profiles
{
    public class UpdateParticipantProfileDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Surname { get; set; } = string.Empty;

        [Range(18, 120)]
        public int Age { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string UserEmail { get; set; } = string.Empty;
    }
}
