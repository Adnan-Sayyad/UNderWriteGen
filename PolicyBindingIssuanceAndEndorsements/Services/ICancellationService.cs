using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public interface ICancellationService
    {
        Cancellation? GetByPolicy(Guid policyId);
        Cancellation? GetById(Guid cancellationId);
        Cancellation Request(CreateCancellationDto dto);
        Cancellation? UpdateStatus(Guid cancellationId, UpdateCancellationStatusDto dto);
    }
}
