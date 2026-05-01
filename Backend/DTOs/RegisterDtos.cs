using System.ComponentModel.DataAnnotations;

namespace ShopHub.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MinLength(3)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter and one number")]
        public string Password { get; set; } = string.Empty;
    }
}