using System.Net;

namespace ComplianceAuditAndQA.Services
{
    public class HttpSubmissionClientService : ISubmissionClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpSubmissionClientService> _logger;

        public HttpSubmissionClientService(
            HttpClient http,
            ILogger<HttpSubmissionClientService> logger)
        {
            _http   = http;
            _logger = logger;
        }

        /// <summary>
        /// Returns <c>false</c> only when the Submission API explicitly returns HTTP 404
        /// (the submission ID does not exist in the submissions table).
        ///
        /// Returns <c>true</c> in every other case:
        ///   • 200 OK  – submission found.
        ///   • non-404 HTTP error (401, 500 …) – cannot verify; allow and log.
        ///   • Network / timeout error – Submission API unreachable; allow and log.
        ///
        /// This means creation is only blocked when we are *certain* the ID is invalid.
        /// </summary>
        public async Task<bool> SubmissionExistsAsync(
            Guid submissionId, CancellationToken ct = default)
        {
            // Hard cap of 5 s so a slow Submission API never stalls compliance writes.
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linked  = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

            try
            {
                var response = await _http.GetAsync($"submissions/{submissionId}", linked.Token);

                // Only block when the submission definitively does not exist.
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation(
                        "Submission not found: SubmissionId={Id}.", submissionId);
                    return false;
                }

                if (response.IsSuccessStatusCode)
                    return true;

                // Non-404 failure (401, 500, …) — cannot verify; allow and warn.
                _logger.LogWarning(
                    "Submission API returned {Status} for SubmissionId={Id}. " +
                    "Allowing operation — could not verify.", response.StatusCode, submissionId);
                return true;
            }
            catch (Exception ex) when (
                ex is HttpRequestException
                    or OperationCanceledException
                    or TaskCanceledException)
            {
                _logger.LogWarning(ex,
                    "Submission API unreachable or timed-out for SubmissionId={Id}. " +
                    "Allowing operation.", submissionId);
                return true;
            }
        }
    }
}
