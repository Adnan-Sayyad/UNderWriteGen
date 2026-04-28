using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public interface IPolicyService
    {
        IEnumerable<Policy> GetAll();
        Policy? GetById(Guid policyId);
        Policy? GetByPolicyNumber(string policyNumber);
        Policy? GetBySubmission(Guid submissionId);
        IEnumerable<Policy> GetByProductLine(string productLine);
        IEnumerable<Policy> GetExpiring(int daysAhead = 30);
        Policy Bind(CreatePolicyDto dto);
        Policy? Update(Guid policyId, UpdatePolicyDto dto);
        Policy? UpdateStatus(Guid policyId, UpdatePolicyStatusDto dto);
        bool Delete(Guid policyId);
    }
}
