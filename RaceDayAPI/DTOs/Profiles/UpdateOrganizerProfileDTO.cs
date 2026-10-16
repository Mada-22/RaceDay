using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Profiles
{
    public class UpdateOrganizerProfileDTO
    {
        [Required]
        [StringLength(100)]
        public string OrganizationName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string UserEmail { get; set; } = string.Empty;
    }
}
