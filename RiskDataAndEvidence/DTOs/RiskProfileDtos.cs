using System.ComponentModel.DataAnnotations;

namespace RiskDataAndEvidence.DTOs;

public class RiskProfileSummaryDto
{
    public Guid RiskID { get; set; }
    public Guid SubmissionID { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public string? RiskNotes { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class RiskProfileDetailDto : RiskProfileSummaryDto
{
    public string AttributesJSON { get; set; } = "{}";
}

public class CreateRiskProfileDto
{
    [Required]
    public Guid SubmissionID { get; set; }

    [Required]
    public string RiskType { get; set; } = string.Empty;

    [Required]
    public string AttributesJSON { get; set; } = "{}";

    public string? RiskNotes { get; set; }
}

public class UpdateRiskProfileDto
{
    [Required]
    public string RiskType { get; set; } = string.Empty;

    [Required]
    public string AttributesJSON { get; set; } = "{}";

    public string? RiskNotes { get; set; }
}
