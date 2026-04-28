using System.ComponentModel.DataAnnotations;

namespace RiskDataAndEvidence.Domain;

public class EvidenceRef
{
    [Key]
    public Guid EvidenceID { get; set; } = Guid.NewGuid();

    public Guid SubmissionID { get; set; }

    // InspectionReport | Medical | Lab | Telematics | ClaimsHistory | Sanctions
    [Required, MaxLength(50)]
    public string EvidenceType { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Provider { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ReferenceNo { get; set; } = string.Empty;

    // Raw result payload from the external provider (Phase-1: reference only)
    public string? ResultJSON { get; set; }

    public DateTime? ReceivedDate { get; set; }

    // Requested | Received | NotAvailable
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Requested";
}
