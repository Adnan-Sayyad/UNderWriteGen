using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.configs.Enums;
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

        public async Task<(IEnumerable<RiskScore> Items, int Total)> GetPagedBySubmissionIdAsync(
            Guid submissionId, int page, int size)
        {
            var query = _context.RiskScores.Where(s => s.SubmissionID == submissionId);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.ScoredDate)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();
            return (items, total);
        }

        public async Task<RiskScore?> GetLatestBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.RiskScores
                .Where(s => s.SubmissionID == submissionId)
                .OrderByDescending(s => s.ScoredDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RiskScore>> GetByBandAsync(Band band)
        {
            return await _context.RiskScores
                .Where(s => s.Band == band)
                .ToListAsync();
        }

        public async Task<(IEnumerable<RiskScore> Items, int Total)> GetPagedByBandAsync(
            Band band, int page, int size)
        {
            var query = _context.RiskScores.Where(s => s.Band == band);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.ScoredDate)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();
            return (items, total);
        }

        public async Task<RiskScore> CreateAsync(RiskScore riskScore)
        {
            riskScore.RiskScoreID = Guid.NewGuid();
            _context.RiskScores.Add(riskScore);
            await _context.SaveChangesAsync();
            return riskScore;
        }

        public async Task<RiskScore> UpsertScoreAsync(
            Guid submissionId, double scoreValue, Band band, string modelVersion)
        {
            // Load all existing records for this submission
            var all = await _context.RiskScores
                .Where(s => s.SubmissionID == submissionId)
                .OrderBy(s => s.ScoredDate)
                .ToListAsync();

            RiskScore record;

            if (all.Count > 0)
            {
                // Keep the oldest record and remove duplicates
                record = all[0];
                if (all.Count > 1)
                    _context.RiskScores.RemoveRange(all.Skip(1));

                // Update the kept record with the deterministic value
                record.ScoreValue   = scoreValue;
                record.Band         = band;
                record.ModelVersion = modelVersion;
                record.ScoredDate   = DateTime.UtcNow;
            }
            else
            {
                record = new RiskScore
                {
                    RiskScoreID  = Guid.NewGuid(),
                    SubmissionID = submissionId,
                    ModelVersion = modelVersion,
                    ScoreValue   = scoreValue,
                    Band         = band,
                    ScoredDate   = DateTime.UtcNow,
                };
                _context.RiskScores.Add(record);
            }

            await _context.SaveChangesAsync();
            return record;
        }
    }
}
