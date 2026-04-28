using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class UWDecisionService : IUWDecisionService
    {
        private readonly UWWorkflowDbContext _db;
        private readonly INotificationClientService _notificationClient;

        public UWDecisionService(UWWorkflowDbContext db, INotificationClientService notificationClient)
        {
            _db = db;
            _notificationClient = notificationClient;
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

            var category = dto.Decision is "Approve" or "Decline" ? "Compliance" : "Referral";
            var message = $"UW Decision '{dto.Decision}' recorded for submission '{dto.SubmissionID}'. Reason: {dto.Reason}";
            _ = _notificationClient.SendAsync(dto.DecidedBy.ToString(), message, category);

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
