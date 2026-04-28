using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Data;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Repositories
{
    public class ReferralRepository : IReferralRepository
    {
        private readonly RulesScoringAndReferralMatrixDbContext _context;

        public ReferralRepository(RulesScoringAndReferralMatrixDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Referral>> GetAllAsync()
        {
            return await _context.Referrals.ToListAsync();
        }

        public async Task<Referral?> GetByIdAsync(Guid id)
        {
            return await _context.Referrals.FirstOrDefaultAsync(r => r.ReferralID == id);
        }

        public async Task<IEnumerable<Referral>> GetBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.Referrals
                .Where(r => r.SubmissionID == submissionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Referral>> GetByStatusAsync(ReferralStatus status)
        {
            return await _context.Referrals
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<Referral> CreateAsync(Referral referral)
        {
            referral.ReferralID = Guid.NewGuid();
            referral.CreatedDate = DateTime.UtcNow;
            referral.Status = ReferralStatus.Pending;
            _context.Referrals.Add(referral);
            await _context.SaveChangesAsync();
            return referral;
        }

        public async Task<IEnumerable<Referral>> GetByAuthorityAsync(RequiredAuthority authority)
        {
            return await _context.Referrals
                .Where(r => r.RequiredAuthority == authority)
                .ToListAsync();
        }

        public async Task<IEnumerable<Referral>> GetByAssignedToAsync(string userId)
        {
            return await _context.Referrals
                .Where(r => r.AssignedTo == userId)
                .ToListAsync();
        }

        public async Task<Referral?> UpdateAsync(Referral referral)
        {
            var existing = await _context.Referrals
                .FirstOrDefaultAsync(r => r.ReferralID == referral.ReferralID);
            if (existing is null) return null;

            existing.AssignedTo = referral.AssignedTo;
            existing.Status = referral.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Referral?> UpdateStatusAsync(Guid id, ReferralStatus status)
        {
            var existing = await _context.Referrals.FirstOrDefaultAsync(r => r.ReferralID == id);
            if (existing is null) return null;

            existing.Status = status;
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
