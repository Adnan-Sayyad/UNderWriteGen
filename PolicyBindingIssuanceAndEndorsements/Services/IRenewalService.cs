using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public interface IRenewalService
    {
        Renewal? GetByPolicy(Guid policyId);
        Renewal? GetById(Guid renewalId);
        IEnumerable<Renewal> GetPending();
        Renewal Create(CreateRenewalDto dto);
        Renewal? UpdateStatus(Guid renewalId, UpdateRenewalStatusDto dto);
    }
}
