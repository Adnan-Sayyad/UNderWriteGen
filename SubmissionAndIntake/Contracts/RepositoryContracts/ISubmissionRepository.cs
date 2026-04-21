using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Contracts.RepositoryContracts
{
    public interface ISubmissionRepository
    {
        Task<IEnumerable<Submission>> GetAllAsync();
        Task<Submission?> GetByIdAsync(Guid id);
        Task<IEnumerable<Submission>> GetByAgentIdAsync(Guid agentId);
        Task<IEnumerable<Submission>> GetByPartyIdAsync(Guid partyId);
        Task<IEnumerable<Submission>> GetByStatusAsync(SubmissionStatus status);
        Task<IEnumerable<Submission>> GetByProductLineAsync(ProductLine productLine);
        Task<Submission> CreateAsync(Submission submission);
        Task<Submission?> UpdateAsync(Submission submission);
        Task<Submission?> UpdateStatusAsync(Guid id, SubmissionStatus status);
        Task<bool> DeleteAsync(Guid id);
    }
}
