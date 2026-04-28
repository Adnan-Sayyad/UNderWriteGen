using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly PolicyDbContext _db;

        public PolicyService(PolicyDbContext db) => _db = db;

        public IEnumerable<Policy> GetAll() => _db.Policies.ToList();

        public Policy? GetById(Guid policyId) =>
            _db.Policies.FirstOrDefault(p => p.PolicyID == policyId);

        public Policy? GetByPolicyNumber(string policyNumber) =>
            _db.Policies.FirstOrDefault(p => p.PolicyNumber == policyNumber);

        public Policy? GetBySubmission(Guid submissionId) =>
            _db.Policies.FirstOrDefault(p => p.SubmissionID == submissionId);

        public IEnumerable<Policy> GetByProductLine(string productLine) =>
            _db.Policies.Where(p => p.ProductLine == productLine).ToList();

        public IEnumerable<Policy> GetExpiring(int daysAhead = 30)
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddDays(daysAhead);
            return _db.Policies
                      .Where(p => p.Status == "Active" && p.ExpiryDate >= now && p.ExpiryDate <= cutoff)
                      .ToList();
        }

        public Policy Bind(CreatePolicyDto dto)
        {
            var policy = new Policy
            {
                SubmissionID = dto.SubmissionID,
                PolicyNumber = dto.PolicyNumber,
                ProductLine = dto.ProductLine,
                CoverageJSON = dto.CoverageJSON,
                InceptionDate = dto.InceptionDate,
                ExpiryDate = dto.ExpiryDate
            };
            _db.Policies.Add(policy);
            _db.SaveChanges();
            return policy;
        }

        public Policy? Update(Guid policyId, UpdatePolicyDto dto)
        {
            var policy = _db.Policies.FirstOrDefault(p => p.PolicyID == policyId);
            if (policy is null) return null;
            policy.ProductLine = dto.ProductLine;
            policy.CoverageJSON = dto.CoverageJSON;
            policy.InceptionDate = dto.InceptionDate;
            policy.ExpiryDate = dto.ExpiryDate;
            _db.SaveChanges();
            return policy;
        }

        public Policy? UpdateStatus(Guid policyId, UpdatePolicyStatusDto dto)
        {
            var policy = _db.Policies.FirstOrDefault(p => p.PolicyID == policyId);
            if (policy is null) return null;
            policy.Status = dto.Status;
            _db.SaveChanges();
            return policy;
        }

        public bool Delete(Guid policyId)
        {
            var policy = _db.Policies.FirstOrDefault(p => p.PolicyID == policyId);
            if (policy is null) return false;
            _db.Policies.Remove(policy);
            _db.SaveChanges();
            return true;
        }
    }
}
