using Microsoft.EntityFrameworkCore;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Data;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly SubmissionAndIntakeDbContext _context;

        public AttachmentRepository(SubmissionAndIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attachment>> GetAllAsync()
        {
            return await _context.Attachments.ToListAsync();
        }

        public async Task<Attachment?> GetByIdAsync(Guid id)
        {
            return await _context.Attachments.FirstOrDefaultAsync(a => a.AttachmentID == id);
        }

        public async Task<IEnumerable<Attachment>> GetBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.Attachments
                .Where(a => a.SubmissionID == submissionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attachment>> GetByDocTypeAsync(DocType docType)
        {
            return await _context.Attachments
                .Where(a => a.DocType == docType)
                .ToListAsync();
        }

        public async Task<Attachment> CreateAsync(Attachment attachment)
        {
            attachment.AttachmentID = Guid.NewGuid();
            attachment.UploadedDate = DateTime.UtcNow;
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var attachment = await _context.Attachments.FirstOrDefaultAsync(a => a.AttachmentID == id);
            if (attachment is null) return false;

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
