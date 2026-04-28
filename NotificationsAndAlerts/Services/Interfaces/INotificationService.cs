using NotificationsAndAlerts.Models.DTOs;

namespace NotificationsAndAlerts.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto);
        Task<NotificationResponseDto?> GetByIdAsync(string id);
        Task<IEnumerable<NotificationResponseDto>> GetAllAsync();
        Task<IEnumerable<NotificationResponseDto>> GetByUserAsync(string userId);
        Task<bool> MarkAsReadAsync(string id);
        Task<bool> DismissAsync(string id);
        Task<bool> DeleteAsync(string id);
    }
}
