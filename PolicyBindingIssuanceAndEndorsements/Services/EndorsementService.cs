using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class EndorsementService : IEndorsementService
    {
        private readonly PolicyDbContext _db;

        public EndorsementService(PolicyDbContext db) => _db = db;

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
