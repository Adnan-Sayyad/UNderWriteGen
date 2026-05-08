using Microsoft.Data.SqlClient;
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
        private readonly IConfiguration _config;

        public NotificationService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto, string senderEmail)
        {
            var notification = new Notification
            {
                NotificationID = await GenerateIdAsync(),
                Mail           = dto.RecipientEmail,
                SenderEmail    = senderEmail,
                Message        = dto.Message,
                Category       = dto.Category,
                Status         = "Unread",
                CreatedDate    = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            return ToDto(notification);
        }

        public async Task<NotificationResponseDto?> GetByIdAsync(string id)
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

        // Returns every notification where the given email is either the recipient or the sender.
        public async Task<IEnumerable<NotificationResponseDto>> GetByParticipantAsync(string email)
        {
            var list = await _db.Notifications
                .Where(n => n.Mail == email || n.SenderEmail == email)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
            return list.Select(ToDto);
        }

        public async Task<IEnumerable<NotificationResponseDto>> BroadcastAsync(
            BroadcastNotificationDto dto, string senderEmail)
        {
            // 1. Fetch recipient emails from the IAM database by role
            var emails = await GetEmailsByRoleAsync(dto.RecipientGroup);

            if (!emails.Any())
                return Enumerable.Empty<NotificationResponseDto>();

            // 2. Get the current count once, then increment locally for each notification
            var today  = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"NTF-{today}-";
            var baseCount = await _db.Notifications
                .Where(n => n.NotificationID.StartsWith(prefix))
                .CountAsync();

            // 3. Create one notification per recipient
            var created = new List<Notification>();
            var seq = baseCount;
            foreach (var email in emails)
            {
                seq++;
                var notification = new Notification
                {
                    NotificationID = $"{prefix}{seq:D4}",
                    Mail           = email,
                    SenderEmail    = senderEmail,
                    Message        = dto.Message,
                    Category       = dto.Category,
                    Status         = "Unread",
                    CreatedDate    = DateTime.UtcNow
                };
                _db.Notifications.Add(notification);
                created.Add(notification);
            }

            await _db.SaveChangesAsync();
            return created.Select(ToDto);
        }

        // Queries the IAM database for active user emails by role.
        private async Task<List<string>> GetEmailsByRoleAsync(string group)
        {
            var connStr = _config.GetConnectionString("IamConnection");
            var emails  = new List<string>();

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            string sql;
            SqlCommand cmd;

            if (group.Equals("Everyone", StringComparison.OrdinalIgnoreCase))
            {
                sql = "SELECT Email FROM Users WHERE IsDeleted = 0 AND Email IS NOT NULL";
                cmd = new SqlCommand(sql, conn);
            }
            else
            {
                sql = "SELECT Email FROM Users WHERE Role = @role AND IsDeleted = 0 AND Email IS NOT NULL";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@role", group);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var email = reader.GetString(0);
                if (!string.IsNullOrWhiteSpace(email))
                    emails.Add(email);
            }

            return emails;
        }

        public async Task<bool> MarkAsReadAsync(string id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            n.Status = "Read";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DismissAsync(string id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            n.Status = "Dismissed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            _db.Notifications.Remove(n);
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateIdAsync()
        {
            var today  = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"NTF-{today}-";

            var countToday = await _db.Notifications
                .Where(n => n.NotificationID.StartsWith(prefix))
                .CountAsync();

            return $"{prefix}{(countToday + 1):D4}";
        }

        private static NotificationResponseDto ToDto(Notification n) => new()
        {
            NotificationID = n.NotificationID,
            RecipientEmail = n.Mail,
            SenderEmail    = n.SenderEmail,
            Message        = n.Message,
            Category       = n.Category,
            Status         = n.Status,
            CreatedDate    = n.CreatedDate
        };
    }
}
