using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Contracts.RepositoryContracts
{
    public interface IAttachmentRepository
    {
        Task<IEnumerable<Attachment>> GetAllAsync();
        Task<Attachment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Attachment>> GetBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<Attachment>> GetByDocTypeAsync(DocType docType);
        Task<Attachment> CreateAsync(Attachment attachment);
        Task<bool> DeleteAsync(Guid id);
    }
}
