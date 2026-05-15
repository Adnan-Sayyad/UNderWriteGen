using Microsoft.EntityFrameworkCore;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Data;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Repositories
{
    public class CompletenessCheckRepository : ICompletenessCheckRepository
    {
        private readonly SubmissionAndIntakeDbContext _context;

        public CompletenessCheckRepository(SubmissionAndIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompletenessCheck>> GetAllAsync()
        {
            return await _context.CompletenessChecks.ToListAsync();
        }

        public async Task<CompletenessCheck?> GetByIdAsync(Guid id)
        {
            return await _context.CompletenessChecks.FirstOrDefaultAsync(c => c.CheckID == id);
        }

        public async Task<IEnumerable<CompletenessCheck>> GetBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.CompletenessChecks
                .Where(c => c.SubmissionID == submissionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompletenessCheck>> GetByStatusAsync(CheckStatus status)
        {
            return await _context.CompletenessChecks
                .Where(c => c.Status == status)
                .ToListAsync();
        }

        public async Task<CompletenessCheck> CreateAsync(CompletenessCheck check)
        {
            check.CheckID    = Guid.NewGuid();
            check.CheckedDate ??= DateTime.UtcNow;
            // Status is set by the caller (Service layer) — do NOT override here
            _context.CompletenessChecks.Add(check);
            await _context.SaveChangesAsync();
            return check;
        }

        public async Task<CompletenessCheck?> UpdateAsync(CompletenessCheck check)
        {
            var existing = await _context.CompletenessChecks.FirstOrDefaultAsync(c => c.CheckID == check.CheckID);
            if (existing is null) return null;

            existing.MissingItemsJSON = check.MissingItemsJSON;
            existing.Status = check.Status;
            existing.CheckedDate = check.CheckedDate;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<CompletenessCheck?> UpdateStatusAsync(Guid id, CheckStatus status)
        {
            var existing = await _context.CompletenessChecks.FirstOrDefaultAsync(c => c.CheckID == id);
            if (existing is null) return null;

            existing.Status = status;
            if (status == CheckStatus.Complete)
                existing.CheckedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
