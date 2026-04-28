using System.ComponentModel.DataAnnotations;

namespace NotificationsAndAlerts.Models.DTOs
{
    public class CreateNotificationDto
    {
        [Required] public string UserID { get; set; } = string.Empty;
        [Required] public string Message { get; set; } = string.Empty;

        // Allowed: Referral / SLA / Subjectivity / Quote / Compliance
        [Required] public string Category { get; set; } = string.Empty;
    }

    public class NotificationResponseDto
    {
        public int NotificationID { get; set; }
        public string UserID { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
