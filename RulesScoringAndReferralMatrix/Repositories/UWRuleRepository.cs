using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Data;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Repositories
{
    public class UWRuleRepository : IUWRuleRepository
    {
        private readonly RulesScoringAndReferralMatrixDbContext _context;

        public UWRuleRepository(RulesScoringAndReferralMatrixDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UWRule>> GetAllAsync()
        {
            return await _context.Rules.ToListAsync();
        }

        public async Task<UWRule?> GetByIdAsync(Guid id)
        {
            return await _context.Rules.FirstOrDefaultAsync(r => r.UWRuleID == id);
        }

        public async Task<IEnumerable<UWRule>> GetByProductLineAsync(string productLine)
        {
            return await _context.Rules
                .Where(r => r.ProductLine == productLine)
                .ToListAsync();
        }

        public async Task<IEnumerable<UWRule>> GetActiveRulesAsync()
        {
            return await _context.Rules
                .Where(r => r.Status == UWStatus.Active)
                .ToListAsync();
        }

        public async Task<UWRule> CreateAsync(UWRule rule)
        {
            rule.UWRuleID = Guid.NewGuid();
            _context.Rules.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<UWRule?> UpdateAsync(UWRule rule)
        {
            var existing = await _context.Rules.FirstOrDefaultAsync(r => r.UWRuleID == rule.UWRuleID);
            if (existing is null) return null;

            existing.ProductLine = rule.ProductLine;
            existing.ExpressionJSON = rule.ExpressionJSON;
            existing.Severity = rule.Severity;
            existing.Status = rule.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var rule = await _context.Rules.FirstOrDefaultAsync(r => r.UWRuleID == id);
            if (rule is null) return false;

            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
