using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UnderwritingWorkflowAndDecisions.Data;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class AiSummaryService : IAiSummaryService
    {
        private readonly IHttpClientFactory      _httpFactory;
        private readonly ISubmissionClientService _submissionClient;
        private readonly UWWorkflowDbContext      _db;
        private readonly string                   _apiKey;

        public AiSummaryService(
            IHttpClientFactory       httpFactory,
            ISubmissionClientService submissionClient,
            UWWorkflowDbContext      db,
            IConfiguration           config)
        {
            _httpFactory      = httpFactory;
            _submissionClient = submissionClient;
            _db               = db;
            _apiKey           = config["Ai:GroqApiKey"] ?? string.Empty;
        }

        public async Task<AiSummaryResult> SummarizeSubmissionAsync(
            Guid submissionId, CancellationToken ct = default)
        {
            var submission = await _submissionClient.GetSubmissionAsync(submissionId, ct);
            var decisions  = await _db.UWDecisions
                                      .Where(d => d.SubmissionID == submissionId)
                                      .OrderByDescending(d => d.DecidedDate)
                                      .ToListAsync(ct);
            var notes      = await _db.UWNotes
                                      .Where(n => n.SubmissionID == submissionId)
                                      .OrderByDescending(n => n.CreatedDate)
                                      .Take(5)
                                      .ToListAsync(ct);

            var prompt = BuildPrompt(submissionId, submission, decisions, notes);
            var summary = await CallGroqAsync(prompt, ct);

            return new AiSummaryResult { Summary = summary, GeneratedAt = DateTime.UtcNow };
        }

        private static string BuildPrompt(
            Guid submissionId,
            SubmissionSummaryData? sub,
            IEnumerable<Models.UWDecision> decisions,
            IEnumerable<Models.UWNote> notes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are an experienced insurance underwriting assistant.");
            sb.AppendLine("Write a concise 3–5 sentence risk summary for the underwriter reviewing this submission.");
            sb.AppendLine("Focus on: key risk factors, whether the submission is standard or needs attention, " +
                          "and any red flags based on the decision and note history.");
            sb.AppendLine("Be professional and factual. Do not invent data not provided.");
            sb.AppendLine();
            sb.AppendLine("=== SUBMISSION DETAILS ===");
            sb.AppendLine($"Submission ID   : {submissionId}");

            if (sub is not null)
            {
                sb.AppendLine($"Product Line    : {sub.ProductLine}");
                sb.AppendLine($"Sum Insured     : {sub.SumInsured:C}");
                sb.AppendLine($"Occupation Type : {sub.OccupationType}");
                sb.AppendLine($"Policy Tenure   : {sub.PolicyTenureMonths} months");
                sb.AppendLine($"Inception Date  : {sub.InceptionDate:dd-MMM-yyyy}");
                sb.AppendLine($"Current Status  : {sub.Status}");
            }
            else
            {
                sb.AppendLine("(Submission service unavailable — working from UW history only)");
            }

            sb.AppendLine();
            sb.AppendLine("=== UW DECISION HISTORY ===");
            if (decisions.Any())
            {
                foreach (var d in decisions)
                    sb.AppendLine($"[{d.DecidedDate:dd-MMM-yyyy}] {d.Decision} — {d.Reason}");
            }
            else
            {
                sb.AppendLine("No decisions recorded yet.");
            }

            sb.AppendLine();
            sb.AppendLine("=== RECENT UW NOTES ===");
            if (notes.Any())
            {
                foreach (var n in notes)
                    sb.AppendLine($"[{n.CreatedDate:dd-MMM-yyyy}] {n.NoteText}");
            }
            else
            {
                sb.AppendLine("No notes recorded yet.");
            }

            return sb.ToString();
        }

        private async Task<string> CallGroqAsync(string prompt, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "AI summary unavailable: Ai:GroqApiKey is not configured.";

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
            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Groq {(int)response.StatusCode}: {errBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<GroqResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            return result?.Choices?.FirstOrDefault()?.Message?.Content
                   ?? "AI summary generation returned an empty response.";
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
}
