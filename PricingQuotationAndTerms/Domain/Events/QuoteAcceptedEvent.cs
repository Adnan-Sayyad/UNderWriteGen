namespace PricingQuotationAndTerms.Domain.Events;

/// <summary>
/// Raised when a customer accepts a quote.
/// Downstream modules (PolicyBinding, Notifications) would listen for this.
/// </summary>
public sealed class QuoteAcceptedEvent
{
    public Guid     QuoteId      { get; }
    public Guid     SubmissionId { get; }
    public decimal  TotalPremium { get; }
    public DateTime AcceptedAt   { get; }

    public QuoteAcceptedEvent(Guid quoteId, Guid submissionId, decimal totalPremium)
    {
        QuoteId      = quoteId;
        SubmissionId = submissionId;
        TotalPremium = totalPremium;
        AcceptedAt   = DateTime.UtcNow;
    }
}
