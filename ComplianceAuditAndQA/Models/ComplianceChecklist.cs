namespace ComplianceAuditAndQA.Models
{
    public class ComplianceChecklist
    {
        public Guid   ChecklistId   { get; set; }
        public Guid   SubmissionId  { get; set; }

        // ── JSON array of checklist items ─────────────────────────
        public string ItemsJson     { get; set; } = string.Empty;

        public string? CompletedBy  { get; set; }
        public DateTime? CompletedDate { get; set; }

        // ── Status: Pending | InProgress | Completed ──────────────
        public string Status        { get; set; } = "Pending";

        // ── Audit fields ──────────────────────────────────────────
        public DateTime  CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }
        public bool      IsDeleted  { get; set; } = false;
    }
}
