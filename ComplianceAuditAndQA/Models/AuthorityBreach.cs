namespace ComplianceAuditAndQA.Models
{
    public class AuthorityBreach
    {
        public Guid   BreachId     { get; set; }
        public Guid   SubmissionId { get; set; }

        // ── BreachType: Authority | RuleOverride | PricingTolerance
        public string BreachType   { get; set; } = string.Empty;
        public string Description  { get; set; } = string.Empty;

        public string?   ApprovedBy   { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // ── Status: Pending | Approved | Rejected ─────────────────
        public string Status      { get; set; } = "Pending";

        // ── Audit fields ──────────────────────────────────────────
        public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool      IsDeleted { get; set; } = false;
    }
}
