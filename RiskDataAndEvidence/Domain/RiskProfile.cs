using System.ComponentModel.DataAnnotations;

namespace RiskDataAndEvidence.Domain;

public class RiskProfile
{
    [Key]
    public Guid RiskID { get; set; } = Guid.NewGuid();

    public Guid SubmissionID { get; set; }

    // Life | Health | Property | Auto | Marine | GL
    [Required, MaxLength(50)]
    public string RiskType { get; set; } = string.Empty;

    // Flexible JSON blob for line-specific risk attributes
    [Required]
    public string AttributesJSON { get; set; } = "{}";

    [MaxLength(2000)]
    public string? RiskNotes { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
