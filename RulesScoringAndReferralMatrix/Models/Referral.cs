using RulesScoringAndReferralMatrix.configs.Enums;

namespace RulesScoringAndReferralMatrix.Models
{
    //o ReferralID
    //o SubmissionID
    //o RaisedBy
    //o Reason
    //o RequiredAuthority
    //o AssignedTo
    //o CreatedDate
    //o Status(Pending/Approved/Rejected)

    public class Referral
    {
        public Guid ReferralID { get; set; }
        public Guid SubmissionID { get; set; }
        public string RaisedBy { get; set; }
        public string Reason { get; set; }
        public RequiredAuthority RequiredAuthority { get; set; }
        public string AssignedTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public ReferralStatus Status { get; set; }
    }
}
