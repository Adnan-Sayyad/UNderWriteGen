using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly PolicyDbContext _db;
        private readonly ISubmissionClientService _submissionClient;
        private readonly INotificationClientService _notificationClient;
        private readonly IComplianceClientService _complianceClient;

        public PolicyService(
            PolicyDbContext db,
            ISubmissionClientService submissionClient,
            INotificationClientService notificationClient,
            IComplianceClientService complianceClient)
        {
            _db = db;
            _submissionClient = submissionClient;
            _notificationClient = notificationClient;
            _complianceClient = complianceClient;
        }

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

        public async Task<string> GenerateUniquePolicyNumberAsync()
        {
            // Format: POL-YYYYMMDD-NNNN  (e.g. POL-20260521-0001)
            var today  = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"POL-{today}-";

            // Pull only today's policy numbers to find the next available counter
            var existing = _db.Policies
                .Where(p => p.PolicyNumber.StartsWith(prefix))
                .Select(p => p.PolicyNumber)
                .ToHashSet();

            int counter = 1;
            string candidate;
            do
            {
                candidate = $"{prefix}{counter:D4}";
                counter++;
            }
            while (existing.Contains(candidate));

            return await Task.FromResult(candidate);
        }

        public async Task<Policy> BindAsync(CreatePolicyDto dto)
        {
            var submissionExists = await _submissionClient.SubmissionExistsAsync(dto.SubmissionID);
            if (!submissionExists)
                throw new KeyNotFoundException($"Submission '{dto.SubmissionID}' not found. Cannot bind policy.");

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
            await _db.SaveChangesAsync();

            // Auto-advance submission to PolicyBound.
            _ = _submissionClient.UpdateStatusAsync(dto.SubmissionID, "PolicyBound");

            var message = $"Policy '{policy.PolicyNumber}' has been successfully bound for submission '{dto.SubmissionID}'.";
            _ = _notificationClient.BroadcastAsync("Agent",      message, "Compliance");
            _ = _notificationClient.BroadcastAsync("Operations", message, "Compliance");
            _ = _notificationClient.BroadcastAsync("Compliance", message, "Compliance");

            // Auto-create compliance checklist so Compliance team can begin 4-eyes review.
            _ = _complianceClient.CreateChecklistAsync(dto.SubmissionID);

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
