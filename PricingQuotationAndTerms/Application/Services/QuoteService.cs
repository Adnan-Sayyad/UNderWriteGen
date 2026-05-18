using System.Text.Json;
using PricingQuotationAndTerms.Application.DTOs.External;
using PricingQuotationAndTerms.Application.DTOs.Requests;
using PricingQuotationAndTerms.Application.DTOs.Responses;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;
using PricingQuotationAndTerms.Domain.Entities;
using PricingQuotationAndTerms.Domain.Events;
using PricingQuotationAndTerms.Domain.Repositories;
using PricingQuotationAndTerms.SharedKernel.Enums;

namespace PricingQuotationAndTerms.Application.Services;

public class QuoteService : IQuoteService, IQuoteApi
{
    private readonly IQuoteRepository           _repo;
    private readonly IPricingService            _pricingService;
    private readonly ISubmissionApi             _submissionApi;
    private readonly IRulesApi                  _rulesApi;
    private readonly IAgentApi                  _agentApi;
    private readonly INotificationClientService _notifications;
    private readonly ILogger<QuoteService>      _logger;

    public QuoteService(
        IQuoteRepository           repo,
        IPricingService            pricingService,
        ISubmissionApi             submissionApi,
        IRulesApi                  rulesApi,
        IAgentApi                  agentApi,
        INotificationClientService notifications,
        ILogger<QuoteService>      logger)
    {
        _repo           = repo;
        _pricingService = pricingService;
        _submissionApi  = submissionApi;
        _rulesApi       = rulesApi;
        _agentApi       = agentApi;
        _notifications  = notifications;
        _logger         = logger;
    }

    // ════════════════════════════════════════════════════════════════
    // POST /api/quotes  —  Generate Quote
    // ════════════════════════════════════════════════════════════════
    public async Task<QuoteResponse> GenerateQuoteAsync(
        CreateQuoteRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating quote for SubmissionId={Id}", request.SubmissionId);

        // 1. Fetch submission from Submission module
        var submission = await _submissionApi.GetSubmissionByIdAsync(request.SubmissionId, ct)
            ?? throw new KeyNotFoundException($"Submission '{request.SubmissionId}' not found.");

        // 2. Fetch risk score from Rules module (overrides what's on submission)
        var riskScore = await _rulesApi.GetRiskScoreAsync(request.SubmissionId, ct)
            ?? throw new InvalidOperationException(
                $"No risk score found for submission '{request.SubmissionId}'. " +
                "Run risk evaluation first.");

        // 3. Block unacceptable risk immediately
        if (riskScore.Band.Equals("Unacceptable", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Submission has Unacceptable risk (score={riskScore.ScoreValue}). Quote declined.");

        // 4. Fetch agent info for discount calculation
        var agent = await _agentApi.GetAgentByIdAsync(submission.AgentId, ct);

        // 5. Build pricing inputs
        var pricingInput = new SubmissionPricingInput
        {
            SubmissionId       = submission.Id,
            PartyId            = submission.PartyId,
            AgentId            = submission.AgentId,
            ProductLine        = submission.ProductLine,
            SumInsured         = submission.SumInsured,
            PolicyTenureMonths = submission.PolicyTenureMonths,
            OccupationType     = submission.OccupationType,
            InceptionDate      = submission.InceptionDate,
            CoverageJson       = submission.CoverageJson,
            IsRenewal          = submission.IsRenewal,
            IsPreferredAgent   = agent?.IsPreferredAgent ?? false
        };

        var riskInput = new RiskScoreInput
        {
            SubmissionId = submission.Id,
            ScoreValue   = riskScore.ScoreValue,
            Band         = riskScore.Band,
            ModelVersion = riskScore.ModelVersion
        };

        // 6. Run pricing engine (reads all rates from DB)
        var pricing = await _pricingService.CalculatePremiumAsync(pricingInput, riskInput, ct);

        // 7. Serialize breakdown to JSON
        string loadingsJson  = JsonSerializer.Serialize(new
            { pricing.RiskLoading, pricing.OccupationLoading });
        string discountsJson = JsonSerializer.Serialize(new
            { pricing.TenureDiscount, pricing.LoyaltyDiscount, pricing.AgentDiscount });
        string taxesJson = JsonSerializer.Serialize(new
            { GstRate = "18%", GstAmount = pricing.TaxAmount });

        // 8. Increment version number for re-quotes
        int versionNo = await _repo.GetLatestVersionAsync(submission.Id, ct) + 1;

        // 9. Re-quote logic: expire all Draft quotes for this submission
        if (versionNo > 1)
        {
            var existingQuotes = await _repo.GetBySubmissionIdAsync(submission.Id, ct);
            foreach (var old in existingQuotes.Where(q => q.Status == QuoteStatus.Draft))
            {
                old.MarkExpired();
                await _repo.UpdateAsync(old, ct);
                _logger.LogInformation("Expired old Draft quote {QuoteId} (v{Ver}) on re-quote", old.Id, old.VersionNo);
            }
        }

        // 10. Create new quote entity
        var quote = new Quote(
            submissionId  : submission.Id,
            basePremium   : pricing.BasePremium,
            totalPremium  : pricing.TotalPremium,
            versionNo     : versionNo,
            loadingsJson  : loadingsJson,
            discountsJson : discountsJson,
            taxesJson     : taxesJson,
            validUntil    : DateTime.UtcNow.AddDays(pricing.QuoteValidityDays));

        await _repo.AddAsync(quote, ct);

        // 11. Domain event
        var evt = new QuoteGeneratedEvent(quote.Id, quote.SubmissionId, quote.TotalPremium, versionNo);
        _logger.LogInformation(
            "[EVENT] QuoteGenerated → QuoteId={QId}, Premium={P}, RiskBand={B}, Version={V}, AgentPreferred={A}",
            evt.QuoteId, evt.TotalPremium, riskScore.Band, versionNo, pricingInput.IsPreferredAgent);

        // Auto-advance submission to Quoted so every module sees the correct state.
        _ = _submissionApi.UpdateSubmissionStatusAsync(submission.Id, "Quoted", ct);

        // PDF §2.11: quote generated — agent presents, underwriter reviews.
        var quoteMsg = $"Quote v{versionNo} generated for submission '{submission.Id}'. Total premium: {quote.TotalPremium:C}. Valid until {quote.ValidUntil:yyyy-MM-dd}.";
        _ = _notifications.BroadcastAsync("Agent",       quoteMsg, "Quote");
        _ = _notifications.BroadcastAsync("Underwriter", quoteMsg, "Quote");

        return MapToResponse(quote);
    }

    // ════════════════════════════════════════════════════════════════
    // GET /api/quotes/{quoteId}
    // ════════════════════════════════════════════════════════════════
    public async Task<QuoteResponse?> GetQuoteByIdAsync(Guid quoteId, CancellationToken ct = default)
    {
        var q = await _repo.GetByIdAsync(quoteId, ct);
        return q is null ? null : MapToResponse(q);
    }

    // ════════════════════════════════════════════════════════════════
    // GET /api/quotes/submission/{submissionId}/latest
    // ════════════════════════════════════════════════════════════════
    public async Task<QuoteResponse?> GetLatestQuoteAsync(Guid submissionId, CancellationToken ct = default)
    {
        var quotes = await _repo.GetBySubmissionIdAsync(submissionId, ct);
        var latest = quotes.FirstOrDefault(); // Repo orders by VersionNo DESC
        return latest is null ? null : MapToResponse(latest);
    }

    // ════════════════════════════════════════════════════════════════
    // POST /api/quotes/{quoteId}/accept
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> AcceptQuoteAsync(Guid quoteId, CancellationToken ct = default)
    {
        var quote = await _repo.GetByIdAsync(quoteId, ct)
            ?? throw new KeyNotFoundException($"Quote '{quoteId}' not found.");

        quote.Accept();
        await _repo.UpdateAsync(quote, ct);

        var evt = new QuoteAcceptedEvent(quote.Id, quote.SubmissionId, quote.TotalPremium);
        _logger.LogInformation(
            "[EVENT] QuoteAccepted → QuoteId={QId}, SubmissionId={SId}, Premium={P}",
            evt.QuoteId, evt.SubmissionId, evt.TotalPremium);

        // Submission stays Quoted; Operations proceed to bind the policy.
        _ = _submissionApi.UpdateSubmissionStatusAsync(quote.SubmissionId, "Quoted", ct);

        var msg = $"Quote '{quote.Id}' accepted for submission '{quote.SubmissionId}'. Total premium: {quote.TotalPremium:C}. Ready to bind.";
        _ = _notifications.BroadcastAsync("Operations",  msg, "Quote");
        _ = _notifications.BroadcastAsync("Underwriter", msg, "Quote");
        _ = _notifications.BroadcastAsync("Agent",       msg, "Quote");

        return true;
    }

    // ════════════════════════════════════════════════════════════════
    // PATCH /api/quotes/{quoteId}/status
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> UpdateStatusAsync(
        Guid quoteId, UpdateQuoteStatusRequest request, CancellationToken ct = default)
    {
        var quote = await _repo.GetByIdAsync(quoteId, ct)
            ?? throw new KeyNotFoundException($"Quote '{quoteId}' not found.");

        if (!Enum.TryParse<QuoteStatus>(request.Status, ignoreCase: true, out var newStatus))
            throw new ArgumentException(
                $"Invalid status '{request.Status}'. Valid: {string.Join(", ", Enum.GetNames<QuoteStatus>())}");

        switch (newStatus)
        {
            case QuoteStatus.Accepted:  quote.Accept();        break;
            case QuoteStatus.Declined:  quote.Decline();       break;
            case QuoteStatus.Presented: quote.MarkPresented(); break;
            case QuoteStatus.Expired:   quote.MarkExpired();   break;
            default: throw new InvalidOperationException($"Status transition to '{request.Status}' not permitted here.");
        }

        await _repo.UpdateAsync(quote, ct);
        _logger.LogInformation("Quote {QId} → status={Status}. Reason: {R}", quoteId, newStatus, request.Reason ?? "N/A");

        // Targeted notifications per status transition.
        var msg = $"Quote '{quote.Id}' for submission '{quote.SubmissionId}' is now {newStatus}." +
                  (string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Reason: {request.Reason}");
        switch (newStatus)
        {
            case QuoteStatus.Presented:
                _ = _notifications.BroadcastAsync("Agent",       msg, "Quote");
                break;
            case QuoteStatus.Accepted:
                _ = _notifications.BroadcastAsync("Operations",  msg, "Quote");
                _ = _notifications.BroadcastAsync("Underwriter", msg, "Quote");
                break;
            case QuoteStatus.Declined:
            case QuoteStatus.Expired:
                _ = _notifications.BroadcastAsync("Agent",       msg, "Quote");
                _ = _notifications.BroadcastAsync("Underwriter", msg, "Quote");
                break;
        }

        return true;
    }

    // ════════════════════════════════════════════════════════════════
    // PATCH /api/quotes/{quoteId}/terms
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> UpdateTermsAsync(UpdateQuoteTermsRequest request, CancellationToken ct = default)
    {
        var quote = await _repo.GetByIdAsync(request.QuoteId, ct)
            ?? throw new KeyNotFoundException($"Quote '{request.QuoteId}' not found.");

        quote.UpdateTerms(request.TermsJson);
        await _repo.UpdateAsync(quote, ct);
        _logger.LogInformation("Terms updated for QuoteId={QId}", request.QuoteId);
        return true;
    }

    // ════════════════════════════════════════════════════════════════
    // Helpers
    // ════════════════════════════════════════════════════════════════
    public async Task<IEnumerable<QuoteResponse>> GetQuotesBySubmissionIdAsync(
        Guid submissionId, CancellationToken ct = default)
        => (await _repo.GetBySubmissionIdAsync(submissionId, ct)).Select(MapToResponse);

    public async Task<IEnumerable<QuoteResponse>> GetAllQuotesAsync(string? status, CancellationToken ct = default)
        => (await _repo.GetByStatusAsync(status, ct)).Select(MapToResponse);

    public async Task<bool> IsQuoteAcceptedAsync(Guid quoteId, CancellationToken ct = default)
    {
        var q = await _repo.GetByIdAsync(quoteId, ct);
        return q?.Status == QuoteStatus.Accepted;
    }

    // ════════════════════════════════════════════════════════════════
    // Mapper
    // ════════════════════════════════════════════════════════════════
    private static QuoteResponse MapToResponse(Quote q) => new()
    {
        QuoteRef      = GenerateRef(q),
        QuoteId       = q.Id,
        SubmissionId  = q.SubmissionId,
        VersionNo     = q.VersionNo,
        BasePremium   = q.BasePremium,
        TotalPremium  = q.TotalPremium,
        ValidUntil    = q.ValidUntil,
        Status        = q.Status.ToString(),
        LoadingsJson  = q.LoadingsJson,
        DiscountsJson = q.DiscountsJson,
        TaxesJson     = q.TaxesJson
    };

    /// <summary>
    /// Derives a short human-readable reference from the GUID.
    /// Deterministic: same quote always gets the same ref. No DB column or migration needed.
    /// Format: QUO-{year}-{5-digit number}  e.g. QUO-2026-04287
    /// </summary>
    private static string GenerateRef(Quote q)
    {
        var bytes = q.Id.ToByteArray();
        int seq   = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 100000;
        return $"QUO-{q.CreatedAt.Year}-{seq:D5}";
    }

    // ════════════════════════════════════════════════════════════════
    // IQuoteApi — bridge for other microservices
    // ════════════════════════════════════════════════════════════════
    async Task<QuoteDto> IQuoteApi.GenerateQuoteAsync(Guid submissionId, CancellationToken ct)
    {
        var r = await GenerateQuoteAsync(
            new CreateQuoteRequest { SubmissionId = submissionId, RequestedBy = "system" }, ct);
        return ToDto(r);
    }
    Task<bool> IQuoteApi.AcceptQuoteAsync(Guid quoteId, CancellationToken ct)
        => AcceptQuoteAsync(quoteId, ct);
    async Task<QuoteDto?> IQuoteApi.GetQuoteByIdAsync(Guid quoteId, CancellationToken ct)
    {
        var r = await GetQuoteByIdAsync(quoteId, ct); return r is null ? null : ToDto(r);
    }
    async Task<IEnumerable<QuoteDto>> IQuoteApi.GetQuotesBySubmissionIdAsync(Guid submissionId, CancellationToken ct)
    {
        var rs = await GetQuotesBySubmissionIdAsync(submissionId, ct); return rs.Select(ToDto);
    }
    Task<bool> IQuoteApi.IsQuoteAcceptedAsync(Guid quoteId, CancellationToken ct)
        => IsQuoteAcceptedAsync(quoteId, ct);

    private static QuoteDto ToDto(QuoteResponse r) => new()
    {
        Id = r.QuoteId, SubmissionId = r.SubmissionId, VersionNo = r.VersionNo,
        BasePremium = r.BasePremium, TotalPremium = r.TotalPremium, ValidUntil = r.ValidUntil,
        Status = r.Status, LoadingsJson = r.LoadingsJson, DiscountsJson = r.DiscountsJson, TaxesJson = r.TaxesJson
    };
}
