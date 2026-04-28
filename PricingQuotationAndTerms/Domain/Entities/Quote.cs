using PricingQuotationAndTerms.SharedKernel.Enums;

namespace PricingQuotationAndTerms.Domain.Entities;

/// <summary>
/// Core domain entity. Represents an insurance quote generated for a submission.
/// All state changes go through domain methods — never set properties directly.
/// </summary>
public class Quote
{
    // EF Core needs a parameterless constructor (private keeps domain clean)
    private Quote() { }

    public Quote(
        Guid   submissionId,
        decimal basePremium,
        decimal totalPremium,
        int    versionNo,
        string loadingsJson,
        string discountsJson,
        string taxesJson,
        DateTime validUntil)
    {
        Id           = Guid.NewGuid();
        SubmissionId = submissionId;
        BasePremium  = basePremium;
        TotalPremium = totalPremium;
        VersionNo    = versionNo;
        LoadingsJson = loadingsJson;
        DiscountsJson= discountsJson;
        TaxesJson    = taxesJson;
        TermsJson    = string.Empty;
        ValidUntil   = validUntil;
        Status       = QuoteStatus.Draft;
        CreatedAt    = DateTime.UtcNow;
    }

    public Guid     Id           { get; private set; }
    public Guid     SubmissionId { get; private set; }
    public int      VersionNo    { get; private set; }
    public decimal  BasePremium  { get; private set; }
    public decimal  TotalPremium { get; private set; }
    public string   LoadingsJson { get; private set; } = string.Empty;
    public string   DiscountsJson{ get; private set; } = string.Empty;
    public string   TaxesJson    { get; private set; } = string.Empty;
    public string   TermsJson    { get; private set; } = string.Empty;
    public DateTime ValidUntil   { get; private set; }
    public DateTime CreatedAt    { get; private set; }
    public DateTime? AcceptedAt  { get; private set; }
    public QuoteStatus Status    { get; private set; }

    // ── Domain behaviour methods ─────────────────────────────────────

    /// <summary>Marks quote as Accepted. Validates expiry and current state.</summary>
    public void Accept()
    {
        if (Status == QuoteStatus.Accepted)
            throw new InvalidOperationException("Quote is already accepted.");

        if (Status == QuoteStatus.Declined)
            throw new InvalidOperationException("A declined quote cannot be accepted.");

        if (ValidUntil < DateTime.UtcNow)
            throw new InvalidOperationException(
                $"Quote expired on {ValidUntil:dd-MMM-yyyy}. Please generate a new quote.");

        Status     = QuoteStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
    }

    /// <summary>Marks quote as Declined.</summary>
    public void Decline()
    {
        if (Status == QuoteStatus.Accepted)
            throw new InvalidOperationException("An accepted quote cannot be declined.");

        Status = QuoteStatus.Declined;
    }

    /// <summary>Marks quote as Presented (sent to customer).</summary>
    public void MarkPresented()
    {
        if (Status == QuoteStatus.Draft)
            Status = QuoteStatus.Presented;
    }

    /// <summary>Attaches policy terms JSON to the quote.</summary>
    public void UpdateTerms(string termsJson)
    {
        if (string.IsNullOrWhiteSpace(termsJson))
            throw new ArgumentException("Terms JSON cannot be empty.");
        TermsJson = termsJson;
    }

    /// <summary>Expires this quote (called by a background job).</summary>
    public void MarkExpired()
    {
        if (Status is QuoteStatus.Draft or QuoteStatus.Presented)
            Status = QuoteStatus.Expired;
    }
}
