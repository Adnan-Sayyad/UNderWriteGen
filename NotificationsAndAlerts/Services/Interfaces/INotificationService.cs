using NotificationsAndAlerts.Models.DTOs;

namespace NotificationsAndAlerts.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto, string senderEmail);
        Task<NotificationResponseDto?> GetByIdAsync(string id);
        Task<IEnumerable<NotificationResponseDto>> GetAllAsync();

        // Returns all notifications where the user is either sender or recipient.
        Task<IEnumerable<NotificationResponseDto>> GetByParticipantAsync(string email);

        Task<IEnumerable<NotificationResponseDto>> BroadcastAsync(BroadcastNotificationDto dto, string senderEmail);

        Task<bool> MarkAsReadAsync(string id);
        Task<bool> DismissAsync(string id);
        Task<bool> DeleteAsync(string id);
    }
}
