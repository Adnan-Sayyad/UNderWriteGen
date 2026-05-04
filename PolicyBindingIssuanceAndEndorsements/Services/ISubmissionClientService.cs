namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public interface ISubmissionClientService
    {
        Task<bool> SubmissionExistsAsync(Guid submissionId, CancellationToken ct = default);
    }
}
