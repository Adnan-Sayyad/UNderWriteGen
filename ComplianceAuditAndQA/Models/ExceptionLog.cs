namespace ComplianceAuditAndQA.Models
{
    public class ExceptionLog
    {
        public Guid   ExceptionId  { get; set; }
        public Guid   SubmissionId { get; set; }

        // ── Category: Data | Process | Compliance ─────────────────
        public string Category     { get; set; } = string.Empty;
        public string Details      { get; set; } = string.Empty;
        public DateTime LoggedDate { get; set; } = DateTime.UtcNow;

        // ── Status: Open | Closed ─────────────────────────────────
        public string Status       { get; set; } = "Open";

        // ── Audit fields ──────────────────────────────────────────
        public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool      IsDeleted { get; set; } = false;
    }
}
