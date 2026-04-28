using Microsoft.EntityFrameworkCore;
using NotificationsAndAlerts.Data;
using NotificationsAndAlerts.Models.DTOs;
using NotificationsAndAlerts.Models.Entities;
using NotificationsAndAlerts.Services.Interfaces;

namespace NotificationsAndAlerts.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserID = dto.UserID,
                Message = dto.Message,
                Category = dto.Category,
                Status = "Unread",
                CreatedDate = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            return ToDto(notification);
        }

        public async Task<NotificationResponseDto?> GetByIdAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            return n is null ? null : ToDto(n);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllAsync()
        {
            var list = await _db.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
            return list.Select(ToDto);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetByUserAsync(string userId)
        {
            var list = await _db.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
            return list.Select(ToDto);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            n.Status = "Read";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DismissAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            n.Status = "Dismissed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            _db.Notifications.Remove(n);
            await _db.SaveChangesAsync();
            return true;
        }

        private static NotificationResponseDto ToDto(Notification n) => new()
        {
            NotificationID = n.NotificationID,
            UserID = n.UserID,
            Message = n.Message,
            Category = n.Category,
            Status = n.Status,
            CreatedDate = n.CreatedDate
        };
    }
}
