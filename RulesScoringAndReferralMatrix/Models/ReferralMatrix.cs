using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.Models
{
    public class ReferralMatrix
    {
        public Guid ReferralMatrixID { get; set; }
        public string ProductLine { get; set; } = string.Empty;

        // What criterion the row matches against (SumInsured / Class / RiskBand).
        public CriteriaJSON CriteriaJSON { get; set; }

        // Comparison operator: "gt", "gte", "lt", "lte", "eq", "ne".
        // Stored as string so it can stay open for future operators (e.g. between, in).
        public string? Operator { get; set; }

        // Threshold value as a string (e.g. "1000000" for SumInsured, "High" for RiskBand).
        // Frontend renders the right input type based on CriteriaJSON.
        public string? Threshold { get; set; }

        public RequiredAuthority RequiredAuthority { get; set; }
        public UWStatus Status { get; set; }
    }
}
