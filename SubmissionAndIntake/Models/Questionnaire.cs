using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.Models
{
    public class Questionnaire
    {
        public Guid QID { get; set; }
        public Guid SubmissionID { get; set; }
        public string TemplateVersion { get; set; } = string.Empty;
        public string ResponsesJSON { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public QuestionnaireStatus Status { get; set; } = QuestionnaireStatus.Pending;
    }
}
