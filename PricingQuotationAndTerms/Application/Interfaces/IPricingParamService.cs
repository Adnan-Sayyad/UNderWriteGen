using PricingQuotationAndTerms.Application.DTOs.Requests;
using PricingQuotationAndTerms.Application.DTOs.Responses;

namespace PricingQuotationAndTerms.Application.Interfaces;

public interface IPricingParamService
{
    Task<IEnumerable<PricingParamResponse>> GetAllAsync(CancellationToken ct = default);
    Task<PricingParamResponse>              CreateAsync(CreatePricingParamRequest request, CancellationToken ct = default);
    Task<PricingParamResponse>              UpdateAsync(Guid paramId, UpdatePricingParamRequest request, CancellationToken ct = default);
    Task<IEnumerable<PricingParamResponse>> GetByProductLineAsync(string productLine, CancellationToken ct = default);
    Task<IEnumerable<PricingParamResponse>> GetByEffectiveDateAsync(DateTime date, CancellationToken ct = default);
}
