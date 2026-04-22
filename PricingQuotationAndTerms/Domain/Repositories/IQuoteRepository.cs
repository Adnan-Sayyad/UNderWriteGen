using PricingQuotationAndTerms.Domain.Entities;

namespace PricingQuotationAndTerms.Domain.Repositories;

/// <summary>
/// Contract for quote persistence. Lives in Domain so it has NO dependency on EF Core.
/// The actual implementation is in Infrastructure.
/// </summary>
public interface IQuoteRepository
{
    Task AddAsync(Quote quote, CancellationToken ct = default);
    Task<Quote?> GetByIdAsync(Guid quoteId, CancellationToken ct = default);
    Task<IEnumerable<Quote>> GetBySubmissionIdAsync(Guid submissionId, CancellationToken ct = default);
    Task UpdateAsync(Quote quote, CancellationToken ct = default);
    Task<int> GetLatestVersionAsync(Guid submissionId, CancellationToken ct = default);
}
