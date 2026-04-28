using System.ComponentModel.DataAnnotations;

namespace RiskDataAndEvidence.DTOs;

public class EvidenceRefSummaryDto
{
    public Guid EvidenceID { get; set; }
    public Guid SubmissionID { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EvidenceRefDetailDto : EvidenceRefSummaryDto
{
    public string? ResultJSON { get; set; }
}

public class CreateEvidenceRefDto
{
    [Required]
    public Guid SubmissionID { get; set; }

    [Required]
    public string EvidenceType { get; set; } = string.Empty;

    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ReferenceNo { get; set; } = string.Empty;

    public string? ResultJSON { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string Status { get; set; } = "Requested";
}

public class UpdateEvidenceRefDto
{
    [Required]
    public string EvidenceType { get; set; } = string.Empty;

    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ReferenceNo { get; set; } = string.Empty;

    public string? ResultJSON { get; set; }

    public DateTime? ReceivedDate { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
}

public class UpdateEvidenceStatusDto
{
    // Requested | Received | NotAvailable
    [Required]
    public string Status { get; set; } = string.Empty;

    public string? ResultJSON { get; set; }

    public DateTime? ReceivedDate { get; set; }
}
