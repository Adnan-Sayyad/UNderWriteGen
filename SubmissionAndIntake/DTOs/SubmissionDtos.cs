using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.DTOs
{
    public class CreateSubmissionDto
    {
        public string PartyID { get; set; } = string.Empty;
        public string AgentID { get; set; } = string.Empty;
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
    }

    public class UpdateSubmissionDto
    {
        public string PartyID { get; set; } = string.Empty;
        public string AgentID { get; set; } = string.Empty;
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
        public SubmissionStatus Status { get; set; }
    }

    public class SubmissionResponseDto
    {
        public Guid SubmissionID { get; set; }
        public string PartyID { get; set; } = string.Empty;
        public string AgentID { get; set; } = string.Empty;
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public SubmissionStatus Status { get; set; }
    }

    public class UpdateSubmissionStatusDto
    {
        public SubmissionStatus Status { get; set; }
    }
}
