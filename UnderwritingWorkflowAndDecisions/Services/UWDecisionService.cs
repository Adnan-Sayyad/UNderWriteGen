using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class UWDecisionService : IUWDecisionService
    {
        private readonly UWWorkflowDbContext _db;

        public UWDecisionService(UWWorkflowDbContext db) => _db = db;

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
