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

        public async Task<(IEnumerable<UWRule> Items, int Total)> GetPagedAsync(
            int page, int size,
            string? productLine, Severity? severity, UWStatus? status)
        {
            var query = _context.Rules.AsQueryable();
            if (!string.IsNullOrWhiteSpace(productLine)) query = query.Where(r => r.ProductLine == productLine);
            if (severity.HasValue) query = query.Where(r => r.Severity == severity.Value);
            if (status.HasValue)   query = query.Where(r => r.Status   == status.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(r => r.ProductLine)
                .ThenBy(r => r.UWRuleID)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();
            return (items, total);
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
            existing.RuleName = rule.RuleName;
            existing.Description = rule.Description;
            existing.ExpressionJSON = rule.ExpressionJSON;
            existing.Severity = rule.Severity;
            existing.Status = rule.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<IEnumerable<UWRule>> GetBySeverityAsync(Severity severity)
        {
            return await _context.Rules
                .Where(r => r.Severity == severity)
                .ToListAsync();
        }

        public async Task<UWRule?> UpdateStatusAsync(Guid id, UWStatus status)
        {
            var existing = await _context.Rules.FirstOrDefaultAsync(r => r.UWRuleID == id);
            if (existing is null) return null;

            existing.Status = status;
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
