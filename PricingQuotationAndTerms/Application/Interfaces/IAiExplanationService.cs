using PricingQuotationAndTerms.Application.DTOs.Responses;
using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Application.Interfaces;

public interface IAiExplanationService
{
    Task<AiExplanationResult> ExplainQuoteAsync(
        QuoteResponse  quote,
        SubmissionDto? submission,
        CancellationToken ct = default);
}

public class AiExplanationResult
{
    public Guid     QuoteId     { get; set; }
    public string   Explanation { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
