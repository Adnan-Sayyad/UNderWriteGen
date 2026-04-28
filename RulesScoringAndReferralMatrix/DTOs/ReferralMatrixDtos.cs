using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.DTOs
{
    public class CreateReferralMatrixDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public CriteriaJSON CriteriaJSON { get; set; }
        public RequiredAuthority RequiredAuthority { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UpdateReferralMatrixDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public CriteriaJSON CriteriaJSON { get; set; }
        public RequiredAuthority RequiredAuthority { get; set; }
        public UWStatus Status { get; set; }
    }

    public class ReferralMatrixResponseDto
    {
        public Guid ReferralMatrixID { get; set; }
        public string ProductLine { get; set; } = string.Empty;
        public CriteriaJSON CriteriaJSON { get; set; }
        public RequiredAuthority RequiredAuthority { get; set; }
        public UWStatus Status { get; set; }
    }

    public class UpdateMatrixStatusDto
    {
        public UWStatus Status { get; set; }
    }
}
