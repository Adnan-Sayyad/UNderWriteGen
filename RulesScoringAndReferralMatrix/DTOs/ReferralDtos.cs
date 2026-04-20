using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.DTOs
{
    public class CreateReferralDto
    {
        public Guid SubmissionID { get; set; }
        public string RaisedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public RequiredAuthority RequiredAuthority { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
    }

    public class UpdateReferralDto
    {
        public string AssignedTo { get; set; } = string.Empty;
        public ReferralStatus Status { get; set; }
    }

    public class ReferralResponseDto
    {
        public Guid ReferralID { get; set; }
        public Guid SubmissionID { get; set; }
        public string RaisedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public RequiredAuthority RequiredAuthority { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public ReferralStatus Status { get; set; }
    }
}
