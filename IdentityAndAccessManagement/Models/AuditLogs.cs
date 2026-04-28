
namespace IdentityAndAccessManagement.Models
{
    public class AuditLogs
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Metadata { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}