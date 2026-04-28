using System.ComponentModel.DataAnnotations;

namespace IdentityAndAccessManagement.DTOs
{
    public class UpdateUserRolesDto
    {
        [Required(ErrorMessage = "Roles are required.")]
        [MinLength(1, ErrorMessage = "At least one role must be provided.")]
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}