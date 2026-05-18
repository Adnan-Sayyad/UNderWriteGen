using Microsoft.EntityFrameworkCore;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Data;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly SubmissionAndIntakeDbContext _context;

        public SubmissionRepository(SubmissionAndIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Submission>> GetAllAsync()
        {
            return await _context.Submissions
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<Submission?> GetByIdAsync(Guid id)
        {
            return await _context.Submissions.FirstOrDefaultAsync(s => s.SubmissionID == id);
        }

        public async Task<IEnumerable<Submission>> GetByAgentIdAsync(string agentId)
        {
            return await _context.Submissions
                .Where(s => s.AgentID == agentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetByPartyIdAsync(string partyId)
        {
            return await _context.Submissions
                .Where(s => s.PartyID == partyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetByStatusAsync(SubmissionStatus status)
        {
            return await _context.Submissions
                .Where(s => s.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetByProductLineAsync(ProductLine productLine)
        {
            return await _context.Submissions
                .Where(s => s.ProductLine == productLine)
                .ToListAsync();
        }

        public async Task<Submission> CreateAsync(Submission submission)
        {
            submission.SubmissionID = Guid.NewGuid();
            submission.CreatedDate = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Draft;
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<Submission?> UpdateAsync(Submission submission)
        {
            var existing = await _context.Submissions.FirstOrDefaultAsync(s => s.SubmissionID == submission.SubmissionID);
            if (existing is null) return null;

            existing.PartyID = submission.PartyID;
            existing.AgentID = submission.AgentID;
            existing.ProductLine = submission.ProductLine;
            existing.CoverageJSON = submission.CoverageJSON;
            existing.InceptionDate = submission.InceptionDate;
            existing.Status = submission.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Submission?> UpdateStatusAsync(Guid id, SubmissionStatus status)
        {
            var existing = await _context.Submissions.FirstOrDefaultAsync(s => s.SubmissionID == id);
            if (existing is null) return null;

            existing.Status = status;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.SubmissionID == id);
            if (submission is null) return false;

            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
