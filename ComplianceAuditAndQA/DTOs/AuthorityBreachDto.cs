using System.ComponentModel.DataAnnotations;

namespace ComplianceAuditAndQA.DTOs
{
    // ── Response ──────────────────────────────────────────────────
    public class AuthorityBreachDto
    {
        public Guid      BreachId     { get; set; }
        public Guid      SubmissionId { get; set; }
        public string    BreachType   { get; set; } = string.Empty;
        public string    Description  { get; set; } = string.Empty;
        public string?   ApprovedBy   { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string    Status       { get; set; } = string.Empty;
        public DateTime  CreatedAt    { get; set; }
        public DateTime? UpdatedAt    { get; set; }
    }

    // ── Create ────────────────────────────────────────────────────
    public class CreateAuthorityBreachDto
    {
        [Required(ErrorMessage = "SubmissionId is required.")]
        public Guid SubmissionId { get; set; }

        [Required(ErrorMessage = "BreachType is required.")]
        [RegularExpression("^(Authority|RuleOverride|PricingTolerance)$",
            ErrorMessage = "BreachType must be Authority, RuleOverride or PricingTolerance.")]
        public string BreachType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;
    }

    // ── Update ────────────────────────────────────────────────────
    public class UpdateAuthorityBreachDto
    {
        [Required(ErrorMessage = "BreachType is required.")]
        [RegularExpression("^(Authority|RuleOverride|PricingTolerance)$",
            ErrorMessage = "BreachType must be Authority, RuleOverride or PricingTolerance.")]
        public string BreachType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }
    }

    // ── Status patch (Approve / Reject) ───────────────────────────
    public class UpdateBreachStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Pending|Approved|Rejected)$",
            ErrorMessage = "Status must be Pending, Approved or Rejected.")]
        public string Status { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }
    }
}
