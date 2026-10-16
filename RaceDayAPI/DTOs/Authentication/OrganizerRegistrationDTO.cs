using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Authentication
{
    public class OrganizerRegistrationDTO
    {
        [Required]
        [StringLength(55, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

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
    }
}
