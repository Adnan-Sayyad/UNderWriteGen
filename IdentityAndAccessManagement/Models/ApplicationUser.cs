
using Microsoft.AspNetCore.Identity;

namespace IdentityAndAccessManagement.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // ── Custom Properties ─────────────────────────────────────
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                UpdateUserName();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                UpdateUserName();
            }
        }

        // Override Email so that whenever email is set it also updates the UserName.
        // Email is globally unique, so using it as UserName prevents the
        // firstName.lastName collision that occurs when two users share the same name.
        public override string? Email
        {
            get => base.Email;
            set
            {
                base.Email = value;
                UpdateUserName();
            }
        }

        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // ── Navigation Properties ─────────────────────────────────
        public ICollection<AuditLogs> AuditLogs { get; set; } = new List<AuditLogs>();

        // ── Private Helper ────────────────────────────────────────
        private void UpdateUserName()
        {
            // Prefer email as the canonical username — it is guaranteed unique
            // across all registrations (enforced by RegisterAsync email-duplicate check).
            // Fall back to firstName.lastName only before an email is assigned.
            if (!string.IsNullOrWhiteSpace(Email))
                UserName = Email.ToLower();
            else if (!string.IsNullOrWhiteSpace(_firstName) && !string.IsNullOrWhiteSpace(_lastName))
                UserName = $"{_firstName.Trim().ToLower()}.{_lastName.Trim().ToLower()}";
        }
    }
}