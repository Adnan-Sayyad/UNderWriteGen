using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.Models
{
    //o ScoreID
    //o SubmissionID
    //o ModelVersion
    //o ScoreValue
    //o Band(Low/Medium/High)
    //o ScoredDate

    public class RiskScore
    {
        public Guid RiskScoreID { get; set; }
        public Guid SubmissionID { get; set; }
        public string ModelVersion { get; set; } = string.Empty;
        public double ScoreValue { get; set; }
        public Band Band { get; set; }
        public DateTime ScoredDate { get; set; }
    }
}
