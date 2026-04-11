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
        public Guid SubmissionID { get; private set; }
        public string ModelVersion { get; private set; }
        public double ScoreValue { get; private set; }
        public Band Band { get; private set; }
        public DateTime ScoredDate { get; private set; }
    }
}
