using System.ComponentModel.DataAnnotations;

namespace ComplianceAuditAndQA.DTOs
{
    // ── Response ──────────────────────────────────────────────────
    public class ComplianceChecklistDto
    {
        public Guid      ChecklistId   { get; set; }
        public Guid      SubmissionId  { get; set; }
        public string    ItemsJson     { get; set; } = string.Empty;
        public string?   CompletedBy   { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string    Status        { get; set; } = string.Empty;
        public DateTime  CreatedAt     { get; set; }
        public DateTime? UpdatedAt     { get; set; }
    }

    // ── Create ────────────────────────────────────────────────────
    public class CreateComplianceChecklistDto
    {
        [Required(ErrorMessage = "SubmissionId is required.")]
        public Guid SubmissionId { get; set; }

        [Required(ErrorMessage = "ItemsJson is required.")]
        public string ItemsJson { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? CompletedBy { get; set; }

        public DateTime? CompletedDate { get; set; }

        [RegularExpression("^(Pending|InProgress|Completed)$",
            ErrorMessage = "Status must be Pending, InProgress or Completed.")]
        public string Status { get; set; } = "Pending";
    }

    // ── Update ────────────────────────────────────────────────────
    public class UpdateComplianceChecklistDto
    {
        [Required(ErrorMessage = "ItemsJson is required.")]
        public string ItemsJson { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? CompletedBy { get; set; }

        public DateTime? CompletedDate { get; set; }
    }

    // ── Status patch ──────────────────────────────────────────────
    public class UpdateChecklistStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Pending|InProgress|Completed)$",
            ErrorMessage = "Status must be Pending, InProgress or Completed.")]
        public string Status { get; set; } = string.Empty;
    }
}
