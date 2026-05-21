using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.DTOs
{
    public class CreateUWRuleDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public string? RuleName { get; set; }
        public string? Description { get; set; }
        public string ExpressionJSON { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UpdateUWRuleDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public string? RuleName { get; set; }
        public string? Description { get; set; }
        public string ExpressionJSON { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UWRuleResponseDto
    {
        public Guid UWRuleID { get; set; }
        public string ProductLine { get; set; } = string.Empty;
        public string? RuleName { get; set; }
        public string? Description { get; set; }
        public string ExpressionJSON { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UpdateRuleStatusDto
    {
        public UWStatus Status { get; set; }
    }

    public class RuleEvaluationResultDto
    {
        public Guid UWRuleID { get; set; }
        public string? RuleName { get; set; }
        public string ProductLine { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public bool Triggered { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class EvaluateRulesResponseDto
    {
        public Guid SubmissionID { get; set; }
        public IEnumerable<RuleEvaluationResultDto> Results { get; set; } = [];
        public double? RiskScore { get; set; }
        public string? RiskBand { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
