namespace PricingQuotationAndTerms.SharedKernel.Enums;

// Quote lifecycle states
public enum QuoteStatus
{
    Draft       = 1,   // Quote generated, not yet sent
    Presented   = 2,   // Sent to customer for review
    Accepted    = 3,   // Customer accepted
    Declined    = 4,   // Customer declined
    Expired     = 5    // ValidUntil date passed
}

// Risk evaluation band (from Rules module)
public enum RiskBand
{
    Low            = 1,
    Medium         = 2,
    High           = 3,
    Unacceptable   = 4
}

// Submission lifecycle
public enum SubmissionStatus
{
    Draft          = 1,
    IntakeComplete = 2,
    UnderReview    = 3,
    Quoted         = 4,
    Declined       = 5,
    Expired        = 6
}

// Policy lifecycle
public enum PolicyStatus
{
    Active    = 1,
    Cancelled = 2,
    Expired   = 3,
    Lapsed    = 4
}
