using System.ComponentModel.DataAnnotations;

namespace NotificationsAndAlerts.Models.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        [EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required] public string Message { get; set; } = string.Empty;

        // Allowed: Referral / SLA / Subjectivity / Quote / Compliance
        [Required] public string Category { get; set; } = string.Empty;
    }

    public class NotificationResponseDto
    {
        public string NotificationID { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
