namespace RiskDataAndEvidence.Services
{
    public interface INotificationClientService
    {
        /// <summary>Send a notification to one specific recipient email.</summary>
        Task SendAsync(string recipientEmail, string message, string category, CancellationToken ct = default);

        /// <summary>Broadcast a notification to every active user in the given role group
        /// (Agent, Underwriter, UWManager, UWAssistant, PricingAnalyst, Compliance, Operations, Admin)
        /// or to "Everyone".</summary>
        Task BroadcastAsync(string recipientGroup, string message, string category, CancellationToken ct = default);
    }
}
