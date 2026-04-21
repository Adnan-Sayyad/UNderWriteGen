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
        Task<bool> DeleteAsync(Guid id);
    }
}
