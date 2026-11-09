using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter your username or email.")]
        [Display(Name = "Username or Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
