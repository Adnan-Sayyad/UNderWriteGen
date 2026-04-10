namespace IdentityAndAccessManagement.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Metadata { get; set; }
        public bool IsDeleted { get; set; }
    }
}