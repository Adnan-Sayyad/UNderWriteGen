using Microsoft.EntityFrameworkCore;
using PricingQuotationAndTerms.Domain.Entities;
using PricingQuotationAndTerms.Domain.Repositories;
using PricingQuotationAndTerms.Infrastructure.Data;

namespace PricingQuotationAndTerms.Infrastructure.Repositories;

public class PricingParamRepository : IPricingParamRepository
{
    private readonly QuoteDbContext _context;

    public PricingParamRepository(QuoteDbContext context) => _context = context;

    public async Task<IEnumerable<PricingParam>> GetAllAsync(CancellationToken ct = default) =>
        await _context.PricingParams.AsNoTracking()
            .OrderBy(p => p.ProductLine).ThenBy(p => p.ParamName)
            .ToListAsync(ct);

    public async Task<PricingParam?> GetByIdAsync(Guid paramId, CancellationToken ct = default) =>
        await _context.PricingParams.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paramId, ct);

    public async Task<IEnumerable<PricingParam>> GetByProductLineAsync(
        string productLine, CancellationToken ct = default) =>
        await _context.PricingParams.AsNoTracking()
            .Where(p => p.ProductLine == productLine && p.IsActive)
            .OrderBy(p => p.ParamName)
            .ToListAsync(ct);

    // Returns all active params effective on the given date
    public async Task<IEnumerable<PricingParam>> GetByEffectiveDateAsync(
        DateTime date, CancellationToken ct = default) =>
        await _context.PricingParams.AsNoTracking()
            .Where(p => p.IsActive
                     && p.EffectiveFrom <= date
                     && (p.EffectiveTo == null || p.EffectiveTo >= date))
            .OrderBy(p => p.ProductLine).ThenBy(p => p.ParamName)
            .ToListAsync(ct);

    public async Task<PricingParam?> GetActiveParamAsync(
        string productLine, string paramName, CancellationToken ct = default) =>
        await _context.PricingParams.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductLine == productLine
                                   && p.ParamName   == paramName
                                   && p.IsActive, ct);

    public async Task AddAsync(PricingParam param, CancellationToken ct = default)
    {
        await _context.PricingParams.AddAsync(param, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PricingParam param, CancellationToken ct = default)
    {
        _context.PricingParams.Update(param);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid paramId, CancellationToken ct = default)
    {
        var param = await _context.PricingParams.FindAsync([paramId], ct);
        if (param is null) return;
        _context.PricingParams.Remove(param);
        await _context.SaveChangesAsync(ct);
    }
}
