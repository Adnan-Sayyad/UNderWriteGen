using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.DTOs
{
    public class CreateQuestionnaireDto
    {
        public Guid SubmissionID { get; set; }
        public string TemplateVersion { get; set; } = string.Empty;
        public string ResponsesJSON { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
    }

    public class UpdateQuestionnaireDto
    {
        public string TemplateVersion { get; set; } = string.Empty;
        public string ResponsesJSON { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
    }

    public class UpdateQuestionnaireStatusDto
    {
        public QuestionnaireStatus Status { get; set; }
    }

    public class QuestionnaireResponseDto
    {
        public Guid QID { get; set; }
        public Guid SubmissionID { get; set; }
        public string TemplateVersion { get; set; } = string.Empty;
        public string ResponsesJSON { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public QuestionnaireStatus Status { get; set; }
    }

    public class QuestionnaireTemplateDto
    {
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
