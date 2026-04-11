using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.Models
{

    //o RuleID
    //o ProductLine
    //o ExpressionJSON
    //o Severity(Block/Refer/Load/Info)
    //o Status(Active/Inactive)
    public class UWRule
    {
        public Guid UWRuleID { get; set; }
        public string ProductLine { get; private set; }
        public string ExpressionJSON { get; private set; }
        public Severity Severity { get; set; }
        public UWStatus Status { get; set; }
    }
}
