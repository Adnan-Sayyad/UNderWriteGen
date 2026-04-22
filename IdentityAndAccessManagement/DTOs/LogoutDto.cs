using System.ComponentModel.DataAnnotations;

namespace IdentityAndAccessManagement.DTOs
{
    public class LogoutDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }
    }
}
