using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class UWDecisionService : IUWDecisionService
    {
        private readonly UWWorkflowDbContext _db;
        private readonly INotificationClientService _notificationClient;
        private readonly ISubmissionClientService _submissionClient;

        public UWDecisionService(
            UWWorkflowDbContext db,
            INotificationClientService notificationClient,
            ISubmissionClientService submissionClient)
        {
            _db = db;
            _notificationClient = notificationClient;
            _submissionClient = submissionClient;
        }

        public IEnumerable<UWDecision> GetBySubmission(Guid submissionId) =>
            _db.UWDecisions.Where(d => d.SubmissionID == submissionId).ToList();

        public UWDecision? GetById(Guid decisionId) =>
            _db.UWDecisions.FirstOrDefault(d => d.DecisionID == decisionId);

        public IEnumerable<UWDecision> GetByDecidedUser(Guid userId) =>
            _db.UWDecisions.Where(d => d.DecidedBy == userId).ToList();

        public IEnumerable<UWDecision> GetByType(string decision) =>
            _db.UWDecisions.Where(d => d.Decision == decision).ToList();

        public UWDecision Add(CreateUWDecisionDto dto)
        {
            var decision = new UWDecision
            {
                SubmissionID = dto.SubmissionID,
                Decision = dto.Decision,
                Reason = dto.Reason,
                DecidedBy = dto.DecidedBy
            };
            _db.UWDecisions.Add(decision);
            _db.SaveChanges();

            var message = $"UW Decision '{dto.Decision}' recorded for submission '{dto.SubmissionID}'. Reason: {dto.Reason}";
            switch (dto.Decision)
            {
                case "Approve":
                    // Auto-advance submission to Approved so PricingAnalyst can generate a quote.
                    _ = _submissionClient.UpdateStatusAsync(dto.SubmissionID, "Approved");
                    _ = _notificationClient.BroadcastAsync("Agent",          message, "Compliance");
                    _ = _notificationClient.BroadcastAsync("PricingAnalyst", message, "Quote");
                    _ = _notificationClient.BroadcastAsync("Operations",     message, "Compliance");
                    _ = _notificationClient.BroadcastAsync("Compliance",     message, "Compliance");
                    break;
                case "Decline":
                    // Auto-advance submission to Declined.
                    _ = _submissionClient.UpdateStatusAsync(dto.SubmissionID, "Declined");
                    _ = _notificationClient.BroadcastAsync("Agent",      message, "Compliance");
                    _ = _notificationClient.BroadcastAsync("Compliance", message, "Compliance");
                    break;
                case "Refer":
                    _ = _notificationClient.BroadcastAsync("UWManager", message, "Referral");
                    break;
                case "MoreInfo":
                    _ = _notificationClient.BroadcastAsync("Agent",       message, "Subjectivity");
                    _ = _notificationClient.BroadcastAsync("UWAssistant", message, "Subjectivity");
                    break;
            }

            return decision;
        }

        public UWDecision? Update(Guid decisionId, UpdateUWDecisionDto dto)
        {
            var decision = _db.UWDecisions.FirstOrDefault(d => d.DecisionID == decisionId);
            if (decision is null) return null;
            decision.Decision = dto.Decision;
            decision.Reason = dto.Reason;
            _db.SaveChanges();
            return decision;
        }
    }
}
