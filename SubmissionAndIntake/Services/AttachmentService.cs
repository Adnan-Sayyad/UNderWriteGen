using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentRepository _repository;

        public AttachmentService(IAttachmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AttachmentResponseDto>> GetAllAttachmentsAsync()
        {
            var attachments = await _repository.GetAllAsync();
            return attachments.Select(MapToResponseDto);
        }

        public async Task<AttachmentResponseDto?> GetAttachmentByIdAsync(Guid id)
        {
            var attachment = await _repository.GetByIdAsync(id);
            return attachment is null ? null : MapToResponseDto(attachment);
        }

        public async Task<IEnumerable<AttachmentResponseDto>> GetAttachmentsBySubmissionIdAsync(Guid submissionId)
        {
            var attachments = await _repository.GetBySubmissionIdAsync(submissionId);
            return attachments.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AttachmentResponseDto>> GetAttachmentsByDocTypeAsync(DocType docType)
        {
            var attachments = await _repository.GetByDocTypeAsync(docType);
            return attachments.Select(MapToResponseDto);
        }

        public async Task<AttachmentResponseDto> CreateAttachmentAsync(CreateAttachmentDto dto)
        {
            var attachment = new Attachment
            {
                SubmissionID = dto.SubmissionID,
                DocType = dto.DocType,
                FileURI = dto.FileURI,
                UploadedBy = dto.UploadedBy
            };

            var created = await _repository.CreateAsync(attachment);
            return MapToResponseDto(created);
        }

        public async Task<bool> DeleteAttachmentAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static AttachmentResponseDto MapToResponseDto(Attachment attachment) => new()
        {
            AttachmentID = attachment.AttachmentID,
            SubmissionID = attachment.SubmissionID,
            DocType = attachment.DocType,
            FileURI = attachment.FileURI,
            UploadedBy = attachment.UploadedBy,
            UploadedDate = attachment.UploadedDate
        };
    }
}
