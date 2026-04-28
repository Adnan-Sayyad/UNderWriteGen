using Microsoft.EntityFrameworkCore;
using RiskDataAndEvidence.Domain;
using RiskDataAndEvidence.Persistence;

namespace RiskDataAndEvidence.Repositories;

public class RiskProfileRepository : IRiskProfileRepository
{
    private readonly RiskDataDbContext _db;

    public RiskProfileRepository(RiskDataDbContext db) => _db = db;

    public async Task<IEnumerable<RiskProfile>> GetAllAsync()
        => await _db.RiskProfiles.OrderByDescending(r => r.LastUpdated).ToListAsync();

    public async Task<RiskProfile?> GetByIdAsync(Guid riskId)
        => await _db.RiskProfiles.FindAsync(riskId);

    public async Task<RiskProfile?> GetBySubmissionIdAsync(Guid submissionId)
        => await _db.RiskProfiles.FirstOrDefaultAsync(r => r.SubmissionID == submissionId);

    public async Task<IEnumerable<RiskProfile>> GetByRiskTypeAsync(string riskType)
        => await _db.RiskProfiles
            .Where(r => r.RiskType == riskType)
            .OrderByDescending(r => r.LastUpdated)
            .ToListAsync();

    public async Task AddAsync(RiskProfile profile)
        => await _db.RiskProfiles.AddAsync(profile);

    public Task UpdateAsync(RiskProfile profile)
    {
        _db.RiskProfiles.Update(profile);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RiskProfile profile)
    {
        _db.RiskProfiles.Remove(profile);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
