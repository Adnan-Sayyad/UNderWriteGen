using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.DTOs
{
    public class CreateUWRuleDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public string ExpressionJSON { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UpdateUWRuleDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public string ExpressionJSON { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UWRuleResponseDto
    {
        public Guid UWRuleID { get; set; }
        public string ProductLine { get; set; } = string.Empty;
        public string ExpressionJSON { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }
}
