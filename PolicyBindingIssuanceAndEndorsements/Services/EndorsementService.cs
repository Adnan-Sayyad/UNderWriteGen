using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class EndorsementService : IEndorsementService
    {
        private readonly PolicyDbContext _db;
        private readonly INotificationClientService _notifications;

        public EndorsementService(PolicyDbContext db, INotificationClientService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public IEnumerable<Endorsement> GetByPolicy(Guid policyId) =>
            _db.Endorsements.Where(e => e.PolicyID == policyId).ToList();

        public Endorsement? GetById(Guid endorsementId) =>
            _db.Endorsements.FirstOrDefault(e => e.EndorsementID == endorsementId);

        public IEnumerable<Endorsement> GetByType(string endorsementType) =>
            _db.Endorsements.Where(e => e.EndorsementType == endorsementType).ToList();

        public Endorsement Add(CreateEndorsementDto dto)
        {
            var endorsement = new Endorsement
            {
                PolicyID = dto.PolicyID,
                EndorsementType = dto.EndorsementType,
                ChangesJSON = dto.ChangesJSON,
                EffectiveDate = dto.EffectiveDate,
                PremiumDelta = dto.PremiumDelta
            };
            _db.Endorsements.Add(endorsement);
            _db.SaveChanges();

            // Endorsements require both UW + Operations attention.
            var message = $"Endorsement '{endorsement.EndorsementID}' ({dto.EndorsementType}) proposed for policy '{dto.PolicyID}'. Premium delta: {dto.PremiumDelta:C}.";
            _ = _notifications.BroadcastAsync("Underwriter", message, "Compliance");
            _ = _notifications.BroadcastAsync("Operations",  message, "Compliance");

            return endorsement;
        }

        public Endorsement? Update(Guid endorsementId, UpdateEndorsementDto dto)
        {
            var e = _db.Endorsements.FirstOrDefault(x => x.EndorsementID == endorsementId);
            if (e is null) return null;
            e.EndorsementType = dto.EndorsementType;
            e.ChangesJSON = dto.ChangesJSON;
            e.EffectiveDate = dto.EffectiveDate;
            e.PremiumDelta = dto.PremiumDelta;
            _db.SaveChanges();
            return e;
        }

        public Endorsement? UpdateStatus(Guid endorsementId, UpdateEndorsementStatusDto dto)
        {
            var e = _db.Endorsements.FirstOrDefault(x => x.EndorsementID == endorsementId);
            if (e is null) return null;
            e.Status = dto.Status;
            _db.SaveChanges();

            if (dto.Status is "Approved" or "Posted")
            {
                var message = $"Endorsement '{e.EndorsementID}' on policy '{e.PolicyID}' is now {dto.Status}.";
                _ = _notifications.BroadcastAsync("Agent",      message, "Compliance");
                _ = _notifications.BroadcastAsync("Operations", message, "Compliance");
            }

            return e;
        }

        public bool Delete(Guid endorsementId)
        {
            var e = _db.Endorsements.FirstOrDefault(x => x.EndorsementID == endorsementId);
            if (e is null) return false;
            _db.Endorsements.Remove(e);
            _db.SaveChanges();
            return true;
        }
    }
}
