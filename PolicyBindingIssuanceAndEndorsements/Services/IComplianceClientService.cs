namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public interface IComplianceClientService
    {
        Task CreateChecklistAsync(Guid submissionId, CancellationToken ct = default);
    }
}
