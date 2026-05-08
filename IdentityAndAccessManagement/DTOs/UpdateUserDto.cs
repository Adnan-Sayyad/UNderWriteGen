using System.ComponentModel.DataAnnotations;

namespace IdentityAndAccessManagement.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Enter a valid email address (e.g. user@domain.com).")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Phone must be 10 digits starting with 6, 7, 8 or 9.")]
        public string? PhoneNumber { get; set; }
    }
}