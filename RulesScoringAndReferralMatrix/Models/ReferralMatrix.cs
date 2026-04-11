using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.Models
{
    //o MatrixID
    //o ProductLine
    //o CriteriaJSON(SumInsured/Class/RiskBand)
    //o RequiredAuthority(UW1/UW2/UWManager/Committee)
    //o Status

    public class ReferralMatrix
    {
        public Guid ReferralMatrixID { get; set; }
        public string ProductLine { get; private set; }
        public CriteriaJSON CriteriaJSON { get; private set; }
        public RequiredAuthority RequiredAuthority { get; set; }
        public UWStatus Status { get; set; }
    }
}
