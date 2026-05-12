using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class RenewalService : IRenewalService
    {
        private readonly PolicyDbContext _db;
        private readonly INotificationClientService _notifications;

        public RenewalService(PolicyDbContext db, INotificationClientService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public Renewal? GetByPolicy(Guid policyId) =>
            _db.Renewals.FirstOrDefault(r => r.PolicyID == policyId);

        public Renewal? GetById(Guid renewalId) =>
            _db.Renewals.FirstOrDefault(r => r.RenewalID == renewalId);

        public IEnumerable<Renewal> GetPending() =>
            _db.Renewals.Where(r => r.Status == "Offered").ToList();

        public Renewal Create(CreateRenewalDto dto)
        {
            var renewal = new Renewal
            {
                PolicyID = dto.PolicyID,
                RenewalOfferJSON = dto.RenewalOfferJSON
            };
            _db.Renewals.Add(renewal);
            _db.SaveChanges();

            // PDF §2.11: renewal offered → notify Agent so they can present to customer.
            var message = $"Renewal offer available for policy '{dto.PolicyID}'.";
            _ = _notifications.BroadcastAsync("Agent", message, "Quote");

            return renewal;
        }

        public Renewal? UpdateStatus(Guid renewalId, UpdateRenewalStatusDto dto)
        {
            var r = _db.Renewals.FirstOrDefault(x => x.RenewalID == renewalId);
            if (r is null) return null;
            r.Status = dto.Status;
            _db.SaveChanges();

            if (dto.Status is "Accepted" or "Declined")
            {
                var message = $"Renewal '{r.RenewalID}' on policy '{r.PolicyID}' was {dto.Status}.";
                _ = _notifications.BroadcastAsync("Operations",  message, "Quote");
                _ = _notifications.BroadcastAsync("Underwriter", message, "Quote");
            }

            return r;
        }
    }
}
