using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Data;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Repositories
{
    public class RiskScoreRepository : IRiskScoreRepository
    {
        private readonly RulesScoringAndReferralMatrixDbContext _context;

        public RiskScoreRepository(RulesScoringAndReferralMatrixDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RiskScore>> GetAllAsync()
        {
            return await _context.RiskScores.ToListAsync();
        }

        public async Task<RiskScore?> GetByIdAsync(Guid id)
        {
            return await _context.RiskScores.FirstOrDefaultAsync(s => s.RiskScoreID == id);
        }

        public async Task<IEnumerable<RiskScore>> GetBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.RiskScores
                .Where(s => s.SubmissionID == submissionId)
                .ToListAsync();
        }

        public async Task<RiskScore> CreateAsync(RiskScore riskScore)
        {
            riskScore.RiskScoreID = Guid.NewGuid();
            _context.RiskScores.Add(riskScore);
            await _context.SaveChangesAsync();
            return riskScore;
        }
    }
}
