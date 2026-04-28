using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.Models
{
    public class Submission
    {
        public Guid SubmissionID { get; set; }
        public Guid PartyID { get; set; }
        public Guid AgentID { get; set; }
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public SubmissionStatus Status { get; set; }
    }
}
