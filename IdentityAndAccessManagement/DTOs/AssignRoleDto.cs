using System.ComponentModel.DataAnnotations;

namespace IdentityAndAccessManagement.DTOs
{
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "Admin ID is required.")]
        public Guid AdminId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "At least one role is required.")]
        [MinLength(1, ErrorMessage = "At least one role must be provided.")]
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
