using PricingQuotationAndTerms.Application.DTOs.Requests;
using PricingQuotationAndTerms.Application.DTOs.Responses;

namespace PricingQuotationAndTerms.Application.Interfaces;

public interface IQuoteService
{
    Task<QuoteResponse>              GenerateQuoteAsync(CreateQuoteRequest request, CancellationToken ct = default);
    Task<QuoteResponse?>             GetQuoteByIdAsync(Guid quoteId, CancellationToken ct = default);
    Task<QuoteResponse?>             GetLatestQuoteAsync(Guid submissionId, CancellationToken ct = default);
    Task<bool>                       AcceptQuoteAsync(Guid quoteId, CancellationToken ct = default);
    Task<bool>                       UpdateStatusAsync(Guid quoteId, UpdateQuoteStatusRequest request, CancellationToken ct = default);
    Task<bool>                       UpdateTermsAsync(UpdateQuoteTermsRequest request, CancellationToken ct = default);
    Task<IEnumerable<QuoteResponse>> GetQuotesBySubmissionIdAsync(Guid submissionId, CancellationToken ct = default);
    Task<IEnumerable<QuoteResponse>> GetAllQuotesAsync(string? status, CancellationToken ct = default);
    Task<bool>                       IsQuoteAcceptedAsync(Guid quoteId, CancellationToken ct = default);
}
