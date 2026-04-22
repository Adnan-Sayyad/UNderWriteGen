using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class RenewalService : IRenewalService
    {
        private readonly PolicyDbContext _db;

        public RenewalService(PolicyDbContext db) => _db = db;

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
            return renewal;
        }

        public Renewal? UpdateStatus(Guid renewalId, UpdateRenewalStatusDto dto)
        {
            var r = _db.Renewals.FirstOrDefault(x => x.RenewalID == renewalId);
            if (r is null) return null;
            r.Status = dto.Status;
            _db.SaveChanges();
            return r;
        }
    }
}
