using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Contracts.Interfaces;

/// <summary>
/// Public contract for this module. Other modules (PolicyBinding, Underwriting)
/// call this interface — they never touch our services directly.
/// </summary>
public interface IQuoteApi
{
    Task<QuoteDto>             GenerateQuoteAsync(Guid submissionId, CancellationToken ct = default);
    Task<bool>                 AcceptQuoteAsync(Guid quoteId, CancellationToken ct = default);
    Task<QuoteDto?>            GetQuoteByIdAsync(Guid quoteId, CancellationToken ct = default);
    Task<IEnumerable<QuoteDto>> GetQuotesBySubmissionIdAsync(Guid submissionId, CancellationToken ct = default);
    Task<bool>                 IsQuoteAcceptedAsync(Guid quoteId, CancellationToken ct = default);
}
