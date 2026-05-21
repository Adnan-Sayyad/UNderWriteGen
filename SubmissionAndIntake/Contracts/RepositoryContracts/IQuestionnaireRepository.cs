using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Contracts.RepositoryContracts
{
    public interface IQuestionnaireRepository
    {
        Task<IEnumerable<Questionnaire>> GetAllAsync();
        Task<Questionnaire?> GetByIdAsync(Guid id);
        Task<IEnumerable<Questionnaire>> GetBySubmissionIdAsync(Guid submissionId);
        Task<Questionnaire> CreateAsync(Questionnaire questionnaire);
        Task<Questionnaire?> UpdateAsync(Questionnaire questionnaire);
        Task<Questionnaire?> UpdateStatusAsync(Guid id, QuestionnaireStatus status);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<string>> GetDistinctTemplateVersionsAsync();
    }
}
