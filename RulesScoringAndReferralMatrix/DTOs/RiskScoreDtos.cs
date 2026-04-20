using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.DTOs
{
    public class CreateRiskScoreDto
    {
        public Guid SubmissionID { get; set; }
        public string ModelVersion { get; set; } = string.Empty;
        public double ScoreValue { get; set; }
        public Band Band { get; set; }
        public DateTime ScoredDate { get; set; }
    }

    public class RiskScoreResponseDto
    {
        public Guid RiskScoreID { get; set; }
        public Guid SubmissionID { get; set; }
        public string ModelVersion { get; set; } = string.Empty;
        public double ScoreValue { get; set; }
        public Band Band { get; set; }
        public DateTime ScoredDate { get; set; }
    }
}
