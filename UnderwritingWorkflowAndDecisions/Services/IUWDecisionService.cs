using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public interface IUWDecisionService
    {
        IEnumerable<UWDecision> GetBySubmission(Guid submissionId);
        UWDecision? GetById(Guid decisionId);
        IEnumerable<UWDecision> GetByDecidedUser(Guid userId);
        IEnumerable<UWDecision> GetByType(string decision);
        UWDecision Add(CreateUWDecisionDto dto);
        UWDecision? Update(Guid decisionId, UpdateUWDecisionDto dto);
    }
}
