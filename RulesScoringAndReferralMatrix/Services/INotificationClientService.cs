namespace RulesScoringAndReferralMatrix.Services
{
    public interface INotificationClientService
    {
        Task SendAsync(string userId, string message, string category, CancellationToken ct = default);
    }
}
