using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Data;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Repositories
{
    public class ReferralMatrixRepository : IReferralMatrixRepository
    {
        private readonly RulesScoringAndReferralMatrixDbContext _context;

        public ReferralMatrixRepository(RulesScoringAndReferralMatrixDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReferralMatrix>> GetAllAsync()
        {
            return await _context.ReferralMatrices.ToListAsync();
        }

        public async Task<ReferralMatrix?> GetByIdAsync(Guid id)
        {
            return await _context.ReferralMatrices.FirstOrDefaultAsync(m => m.ReferralMatrixID == id);
        }

        public async Task<IEnumerable<ReferralMatrix>> GetByProductLineAsync(string productLine)
        {
            return await _context.ReferralMatrices
                .Where(m => m.ProductLine == productLine)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReferralMatrix>> GetActiveMatricesAsync()
        {
            return await _context.ReferralMatrices
                .Where(m => m.Status == UWStatus.Active)
                .ToListAsync();
        }

        public async Task<ReferralMatrix> CreateAsync(ReferralMatrix matrix)
        {
            matrix.ReferralMatrixID = Guid.NewGuid();
            _context.ReferralMatrices.Add(matrix);
            await _context.SaveChangesAsync();
            return matrix;
        }

        public async Task<ReferralMatrix?> UpdateAsync(ReferralMatrix matrix)
        {
            var existing = await _context.ReferralMatrices
                .FirstOrDefaultAsync(m => m.ReferralMatrixID == matrix.ReferralMatrixID);
            if (existing is null) return null;

            existing.ProductLine = matrix.ProductLine;
            existing.CriteriaJSON = matrix.CriteriaJSON;
            existing.Operator = matrix.Operator;
            existing.Threshold = matrix.Threshold;
            existing.RequiredAuthority = matrix.RequiredAuthority;
            existing.Status = matrix.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<ReferralMatrix?> UpdateStatusAsync(Guid id, UWStatus status)
        {
            var existing = await _context.ReferralMatrices.FirstOrDefaultAsync(m => m.ReferralMatrixID == id);
            if (existing is null) return null;

            existing.Status = status;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var matrix = await _context.ReferralMatrices.FirstOrDefaultAsync(m => m.ReferralMatrixID == id);
            if (matrix is null) return false;

            _context.ReferralMatrices.Remove(matrix);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
