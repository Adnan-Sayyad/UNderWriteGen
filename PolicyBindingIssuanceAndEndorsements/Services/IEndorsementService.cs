using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public interface IEndorsementService
    {
        IEnumerable<Endorsement> GetByPolicy(Guid policyId);
        Endorsement? GetById(Guid endorsementId);
        IEnumerable<Endorsement> GetByType(string endorsementType);
        Endorsement Add(CreateEndorsementDto dto);
        Endorsement? Update(Guid endorsementId, UpdateEndorsementDto dto);
        Endorsement? UpdateStatus(Guid endorsementId, UpdateEndorsementStatusDto dto);
        bool Delete(Guid endorsementId);
    }
}
