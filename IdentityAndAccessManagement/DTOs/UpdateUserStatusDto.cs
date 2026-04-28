using System.ComponentModel.DataAnnotations;

namespace IdentityAndAccessManagement.DTOs
{
    public class UpdateUserStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Locked|Disabled)$",
            ErrorMessage = "Status must be Active, Locked or Disabled.")]
        public string Status { get; set; } = string.Empty;
    }
}