namespace SubmissionAndIntake.Services
{
    public interface IDistributionValidationService
    {
        Task<bool> AgentExistsAsync(string agentId, CancellationToken ct = default);
        Task<bool> PartyExistsAsync(string partyId, CancellationToken ct = default);
    }
}
