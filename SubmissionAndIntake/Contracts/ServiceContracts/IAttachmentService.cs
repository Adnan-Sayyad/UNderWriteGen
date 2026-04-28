using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Contracts.ServiceContracts
{
    public interface IAttachmentService
    {
        Task<IEnumerable<AttachmentResponseDto>> GetAllAttachmentsAsync();
        Task<AttachmentResponseDto?> GetAttachmentByIdAsync(Guid id);
        Task<IEnumerable<AttachmentResponseDto>> GetAttachmentsBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<AttachmentResponseDto>> GetAttachmentsByDocTypeAsync(DocType docType);
        Task<AttachmentResponseDto> CreateAttachmentAsync(CreateAttachmentDto dto);
        Task<bool> DeleteAttachmentAsync(Guid id);
    }
}
