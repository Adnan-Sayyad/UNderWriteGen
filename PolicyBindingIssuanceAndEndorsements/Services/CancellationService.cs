using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly PolicyDbContext _db;

        public CancellationService(PolicyDbContext db) => _db = db;

        public Cancellation? GetByPolicy(Guid policyId) =>
            _db.Cancellations.FirstOrDefault(c => c.PolicyID == policyId);

        public Cancellation? GetById(Guid cancellationId) =>
            _db.Cancellations.FirstOrDefault(c => c.CancellationID == cancellationId);

        public Cancellation Request(CreateCancellationDto dto)
        {
            var cancellation = new Cancellation
            {
                PolicyID = dto.PolicyID,
                CancelReason = dto.CancelReason,
                CancelDate = dto.CancelDate,
                RefundPremium = dto.RefundPremium
            };
            _db.Cancellations.Add(cancellation);
            _db.SaveChanges();
            return cancellation;
        }

        public Cancellation? UpdateStatus(Guid cancellationId, UpdateCancellationStatusDto dto)
        {
            var c = _db.Cancellations.FirstOrDefault(x => x.CancellationID == cancellationId);
            if (c is null) return null;
            c.Status = dto.Status;
            _db.SaveChanges();
            return c;
        }
    }
}
