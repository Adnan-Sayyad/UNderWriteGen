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

            // 2. Get the next available sequence number (MAX-based, not count-based,
            //    so gaps from deleted notifications don't cause PK collisions).
            var today  = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"NTF-{today}-";
            var nextSeq = await GetNextSeqAsync(prefix);

            // 3. Create one notification per recipient
            var created = new List<Notification>();
            var seq = nextSeq - 1;
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
            var next   = await GetNextSeqAsync(prefix);
            return $"{prefix}{next:D4}";
        }

        // Returns the next available sequence number for the given prefix by
        // inspecting the MAX existing ID rather than the count — this is safe
        // even when earlier IDs have been deleted (which would make count-based
        // generation produce a collision).
        private async Task<int> GetNextSeqAsync(string prefix)
        {
            var latest = await _db.Notifications
                .Where(n => n.NotificationID.StartsWith(prefix))
                .MaxAsync(n => (string?)n.NotificationID);

            if (latest is null) return 1;

            // ID format: NTF-yyyyMMdd-NNNN  →  last segment is the sequence
            var parts = latest.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[^1], out int seq))
                return seq + 1;

            // Fallback (should never happen with well-formed IDs)
            return await _db.Notifications
                .CountAsync(n => n.NotificationID.StartsWith(prefix)) + 1;
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
