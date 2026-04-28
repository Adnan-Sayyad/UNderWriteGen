using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Contracts.ServiceContracts
{
    public interface IQuestionnaireService
    {
        Task<IEnumerable<QuestionnaireResponseDto>> GetAllQuestionnairesAsync();
        Task<QuestionnaireResponseDto?> GetQuestionnaireByIdAsync(Guid id);
        Task<IEnumerable<QuestionnaireResponseDto>> GetQuestionnairesBySubmissionIdAsync(Guid submissionId);
        Task<QuestionnaireResponseDto> CreateQuestionnaireAsync(CreateQuestionnaireDto dto);
        Task<QuestionnaireResponseDto?> UpdateQuestionnaireAsync(Guid id, UpdateQuestionnaireDto dto);
        Task<bool> DeleteQuestionnaireAsync(Guid id);
        Task<IEnumerable<QuestionnaireTemplateDto>> GetTemplatesAsync();
        Task<QuestionnaireTemplateDto?> GetTemplateByVersionAsync(string version);
    }
}
