using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.DTOs
{
    public class CreateSubmissionDto
    {
        public Guid PartyID { get; set; }
        public Guid AgentID { get; set; }
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
    }

    public class UpdateSubmissionDto
    {
        public Guid PartyID { get; set; }
        public Guid AgentID { get; set; }
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
        public SubmissionStatus Status { get; set; }
    }

    public class SubmissionResponseDto
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
