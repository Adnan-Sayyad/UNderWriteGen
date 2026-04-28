using System.ComponentModel.DataAnnotations;

namespace ComplianceAuditAndQA.DTOs
{
    // ── Response ──────────────────────────────────────────────────
    public class ExceptionLogDto
    {
        public Guid      ExceptionId  { get; set; }
        public Guid      SubmissionId { get; set; }
        public string    Category     { get; set; } = string.Empty;
        public string    Details      { get; set; } = string.Empty;
        public DateTime  LoggedDate   { get; set; }
        public string    Status       { get; set; } = string.Empty;
        public DateTime  CreatedAt    { get; set; }
        public DateTime? UpdatedAt    { get; set; }
    }

    // ── Create ────────────────────────────────────────────────────
    public class CreateExceptionLogDto
    {
        [Required(ErrorMessage = "SubmissionId is required.")]
        public Guid SubmissionId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [RegularExpression("^(Data|Process|Compliance)$",
            ErrorMessage = "Category must be Data, Process or Compliance.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Details is required.")]
        public string Details { get; set; } = string.Empty;
    }

    // ── Update ────────────────────────────────────────────────────
    public class UpdateExceptionLogDto
    {
        [Required(ErrorMessage = "Category is required.")]
        [RegularExpression("^(Data|Process|Compliance)$",
            ErrorMessage = "Category must be Data, Process or Compliance.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Details is required.")]
        public string Details { get; set; } = string.Empty;
    }

    // ── Status patch (Open / Close) ───────────────────────────────
    public class UpdateExceptionStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Open|Closed)$",
            ErrorMessage = "Status must be Open or Closed.")]
        public string Status { get; set; } = string.Empty;
    }
}
