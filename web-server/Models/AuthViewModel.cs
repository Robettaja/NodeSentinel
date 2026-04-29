using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class AuthViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]{3,32}$",
        ErrorMessage = "Username must be 3–32 characters and can contain letters, numbers, underscore, dot, or hyphen.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters long")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string RepeatPassword { get; set; } = string.Empty;

    }
}
