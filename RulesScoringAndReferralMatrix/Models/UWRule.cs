using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.Models
{
    public class UWRule
    {
        public Guid UWRuleID { get; set; }
        public string ProductLine { get; set; } = string.Empty;

        // Friendly display fields for UI — nullable so existing rows remain valid.
        public string? RuleName { get; set; }
        public string? Description { get; set; }

        // Structured JSON: { "logic": "AND" | "OR", "conditions": [ { field, type, operator, value } ] }
        public string ExpressionJSON { get; set; } = string.Empty;

        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }
}
