using NotificationsAndAlerts.Models.DTOs;

namespace NotificationsAndAlerts.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto);
        Task<NotificationResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<NotificationResponseDto>> GetAllAsync();
        Task<IEnumerable<NotificationResponseDto>> GetByUserAsync(string userId);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> DismissAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
