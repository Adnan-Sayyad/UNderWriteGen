using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly PolicyDbContext _db;
        private readonly INotificationClientService _notifications;

        public CancellationService(PolicyDbContext db, INotificationClientService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

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

            var message = $"Cancellation requested for policy '{dto.PolicyID}'. Reason: {dto.CancelReason}. Refund: {dto.RefundPremium:C}.";
            _ = _notifications.BroadcastAsync("Underwriter", message, "Compliance");
            _ = _notifications.BroadcastAsync("Operations",  message, "Compliance");

            return cancellation;
        }

        public Cancellation? UpdateStatus(Guid cancellationId, UpdateCancellationStatusDto dto)
        {
            var c = _db.Cancellations.FirstOrDefault(x => x.CancellationID == cancellationId);
            if (c is null) return null;
            c.Status = dto.Status;
            _db.SaveChanges();

            if (dto.Status is "Approved" or "Posted")
            {
                var message = $"Cancellation '{c.CancellationID}' on policy '{c.PolicyID}' is now {dto.Status}.";
                _ = _notifications.BroadcastAsync("Agent",      message, "Compliance");
                _ = _notifications.BroadcastAsync("Operations", message, "Compliance");
            }

            return c;
        }
    }
}
