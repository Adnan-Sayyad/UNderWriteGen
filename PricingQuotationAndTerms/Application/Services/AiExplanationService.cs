using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PricingQuotationAndTerms.Application.DTOs.Responses;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Application.Services;

public class AiExplanationService : IAiExplanationService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string             _apiKey;

    public AiExplanationService(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _apiKey      = config["Ai:GroqApiKey"] ?? string.Empty;
    }

    public async Task<AiExplanationResult> ExplainQuoteAsync(
        QuoteResponse  quote,
        SubmissionDto? submission,
        CancellationToken ct = default)
    {
        var prompt      = BuildPrompt(quote, submission);
        var explanation = await CallGroqAsync(prompt, ct);

        return new AiExplanationResult
        {
            QuoteId     = quote.QuoteId,
            Explanation = explanation,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static string BuildPrompt(QuoteResponse quote, SubmissionDto? sub)
    {
        // Parse the stored JSON breakdowns the pricing engine saved on the quote
        decimal riskLoading = 0, occupationLoading = 0;
        decimal tenureDiscount = 0, loyaltyDiscount = 0, agentDiscount = 0;
        decimal gstAmount = 0;

        TryParseLoadings(quote.LoadingsJson,   ref riskLoading,    ref occupationLoading);
        TryParseDiscounts(quote.DiscountsJson, ref tenureDiscount, ref loyaltyDiscount, ref agentDiscount);
        TryParseGst(quote.TaxesJson,           ref gstAmount);

        var sb = new StringBuilder();
        sb.AppendLine("You are a professional insurance advisor writing a premium explanation letter.");
        sb.AppendLine("Write 3–5 clear sentences explaining how the insurance premium was calculated.");
        sb.AppendLine("Use simple language that an agent can share directly with the customer.");
        sb.AppendLine("Be accurate, warm, and professional. Do not use technical jargon.");
        sb.AppendLine();
        sb.AppendLine("=== QUOTE DETAILS ===");
        sb.AppendLine($"Quote Reference  : {quote.QuoteRef}");
        sb.AppendLine($"Quote Version    : v{quote.VersionNo}");
        sb.AppendLine($"Valid Until      : {quote.ValidUntil:dd-MMM-yyyy}");
        sb.AppendLine($"Status           : {quote.Status}");
        sb.AppendLine();
        sb.AppendLine("=== SUBMISSION CONTEXT ===");
        if (sub is not null)
        {
            sb.AppendLine($"Product Line     : {sub.ProductLine}");
            sb.AppendLine($"Sum Insured      : {sub.SumInsured:C}");
            sb.AppendLine($"Occupation Type  : {sub.OccupationType}");
            sb.AppendLine($"Policy Tenure    : {sub.PolicyTenureMonths} months");
            sb.AppendLine($"Renewal          : {(sub.IsRenewal ? "Yes (loyal customer)" : "No")}");
        }
        sb.AppendLine();
        sb.AppendLine("=== PREMIUM BREAKDOWN ===");
        sb.AppendLine($"Base Premium          : {quote.BasePremium:C}");
        if (riskLoading       > 0) sb.AppendLine($"Risk Loading          : +{riskLoading:C}");
        if (occupationLoading > 0) sb.AppendLine($"Occupation Loading    : +{occupationLoading:C}");
        if (tenureDiscount    > 0) sb.AppendLine($"Tenure Discount       : -{tenureDiscount:C}");
        if (loyaltyDiscount   > 0) sb.AppendLine($"Loyalty Discount      : -{loyaltyDiscount:C}");
        if (agentDiscount     > 0) sb.AppendLine($"Agent Discount        : -{agentDiscount:C}");
        if (gstAmount         > 0) sb.AppendLine($"GST (18%)             : +{gstAmount:C}");
        sb.AppendLine($"TOTAL PREMIUM         : {quote.TotalPremium:C}");

        return sb.ToString();
    }

    private async Task<string> CallGroqAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return "AI explanation unavailable: Ai:GroqApiKey is not configured.";

        var http = _httpFactory.CreateClient("GroqApi");

        var body = new
        {
            model      = "llama-3.3-70b-versatile",
            max_tokens = 512,
            messages   = new[] { new { role = "user", content = prompt } }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        req.Headers.Add("Authorization", $"Bearer {_apiKey}");
        req.Content = JsonContent.Create(body);

        var response = await http.SendAsync(req, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        return result?.Choices?.FirstOrDefault()?.Message?.Content
               ?? "AI explanation generation returned an empty response.";
    }

    // ── JSON helpers ─────────────────────────────────────────────────────
    private static void TryParseLoadings(string json, ref decimal risk, ref decimal occupation)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            if (doc.TryGetProperty("RiskLoading",       out var r)) risk       = r.GetDecimal();
            if (doc.TryGetProperty("OccupationLoading", out var o)) occupation = o.GetDecimal();
        }
        catch { /* leave defaults */ }
    }

    private static void TryParseDiscounts(string json, ref decimal tenure, ref decimal loyalty, ref decimal agent)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            if (doc.TryGetProperty("TenureDiscount",  out var t)) tenure  = t.GetDecimal();
            if (doc.TryGetProperty("LoyaltyDiscount", out var l)) loyalty = l.GetDecimal();
            if (doc.TryGetProperty("AgentDiscount",   out var a)) agent   = a.GetDecimal();
        }
        catch { /* leave defaults */ }
    }

    private static void TryParseGst(string json, ref decimal gst)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            if (doc.TryGetProperty("GstAmount", out var g)) gst = g.GetDecimal();
        }
        catch { /* leave defaults */ }
    }

    private sealed class GroqResponse
    {
        public Choice[]? Choices { get; set; }
    }
    private sealed class Choice
    {
        public ChoiceMessage? Message { get; set; }
    }
    private sealed class ChoiceMessage
    {
        public string? Content { get; set; }
    }
}
