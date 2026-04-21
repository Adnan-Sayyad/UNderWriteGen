using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Contracts.RepositoryContracts
{
    public interface ICompletenessCheckRepository
    {
        Task<IEnumerable<CompletenessCheck>> GetAllAsync();
        Task<CompletenessCheck?> GetByIdAsync(Guid id);
        Task<IEnumerable<CompletenessCheck>> GetBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<CompletenessCheck>> GetByStatusAsync(CheckStatus status);
        Task<CompletenessCheck> CreateAsync(CompletenessCheck check);
        Task<CompletenessCheck?> UpdateAsync(CompletenessCheck check);
        Task<CompletenessCheck?> UpdateStatusAsync(Guid id, CheckStatus status);
    }
}
