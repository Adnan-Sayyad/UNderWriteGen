namespace PricingQuotationAndTerms.Domain.Events;

/// <summary>
/// Raised when a new quote is successfully generated.
/// In a real microservices system this would be published to a message bus (e.g. RabbitMQ / Azure Service Bus).
/// For now we log it to console — the infrastructure is ready to be swapped.
/// </summary>
public sealed class QuoteGeneratedEvent
{
    public Guid     QuoteId       { get; }
    public Guid     SubmissionId  { get; }
    public decimal  TotalPremium  { get; }
    public int      VersionNo     { get; }
    public DateTime OccurredOn    { get; }

    public QuoteGeneratedEvent(Guid quoteId, Guid submissionId, decimal totalPremium, int versionNo)
    {
        QuoteId      = quoteId;
        SubmissionId = submissionId;
        TotalPremium = totalPremium;
        VersionNo    = versionNo;
        OccurredOn   = DateTime.UtcNow;
    }
}
