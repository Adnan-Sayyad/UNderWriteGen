using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationsAndAlerts.Models.Entities
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationID { get; set; }

        // Logical FK -> User table (User entity lives in IdentityAndAccessManagement microservice)
        [Required]
        public string UserID { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        // Referral / SLA / Subjectivity / Quote / Compliance
        [Required]
        public string Category { get; set; } = string.Empty;

        // Unread / Read / Dismissed
        [Required]
        public string Status { get; set; } = "Unread";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
