using PricingQuotationAndTerms.Domain.Entities;

namespace PricingQuotationAndTerms.Domain.Repositories;

public interface IPricingParamRepository
{
    Task<IEnumerable<PricingParam>> GetAllAsync(CancellationToken ct = default);
    Task<PricingParam?>             GetByIdAsync(Guid paramId, CancellationToken ct = default);
    Task<IEnumerable<PricingParam>> GetByProductLineAsync(string productLine, CancellationToken ct = default);
    Task<IEnumerable<PricingParam>> GetByEffectiveDateAsync(DateTime date, CancellationToken ct = default);
    Task<PricingParam?>             GetActiveParamAsync(string productLine, string paramName, CancellationToken ct = default);
    Task AddAsync(PricingParam param, CancellationToken ct = default);
    Task UpdateAsync(PricingParam param, CancellationToken ct = default);
    Task DeleteAsync(Guid paramId, CancellationToken ct = default);
}
