using IdentityAndAccessManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityAndAccessManagement.Seeders
{
    public class UserSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(
            UserManager<ApplicationUser> userManager,
            ILogger<UserSeeder> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var users = GetSeedUsers();

            foreach (var (user, role, password) in users)
            {
                await SeedUserAsync(user, role, password);
            }
        }

        // ── Private: Seed Single User ─────────────────────────────
        private async Task SeedUserAsync(
            ApplicationUser user, string role, string password)
        {
            try
            {
                // ── Check if user already exists ──────────────────
                var existingUser = await _userManager
                    .FindByEmailAsync(user.Email!);

                if (existingUser != null)
                {
                    _logger.LogInformation(
                        "User '{Email}' already exists — skipping.",
                        user.Email);
                    return;
                }

                // ── Create user ───────────────────────────────────
                var result = await _userManager
                    .CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Failed to create user '{Email}': {Errors}",
                        user.Email,
                        string.Join(", ", result.Errors
                            .Select(e => e.Description)));
                    return;
                }

                // ── Assign role ───────────────────────────────────
                var roleResult = await _userManager
                    .AddToRoleAsync(user, role);

                if (roleResult.Succeeded)
                    _logger.LogInformation(
                        "User '{Email}' created with role '{Role}'.",
                        user.Email, role);
                else
                    _logger.LogError(
                        "Failed to assign role '{Role}' to '{Email}': {Errors}",
                        role, user.Email,
                        string.Join(", ", roleResult.Errors
                            .Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception while seeding user '{Email}'.", user.Email);
                throw;
            }
        }

        // ── Seed Users Data ───────────────────────────────────────
        private static List<(ApplicationUser User, string Role, string Password)>
            GetSeedUsers()
        {
            return new List<(ApplicationUser, string, string)>
            {
                // ── 1. Admin ──────────────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "Shiva",
                    LastName    = "Molige",
                    Email       = "moligeshiva@admin.com",
                    PhoneNumber = "7981558584",
                    Role        = "Admin",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "Admin",
                "Admin@shiva2781"),

                // ── 2. Underwriter ────────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "Sarah",
                    LastName    = "Mitchell",
                    Email       = "sarah.mitchell@underwritepro.com",
                    PhoneNumber = "9826769823",
                    Role        = "Underwriter",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "Underwriter",
                "Uw@Sarah123!"),

                // ── 3. Agent ──────────────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "James",
                    LastName    = "Carter",
                    Email       = "james.carter@underwritepro.com",
                    PhoneNumber = "8976624534",
                    Role        = "Agent",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "Agent",
                "Agent@James123!"),

                // ── 4. Compliance ─────────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "Emily",
                    LastName    = "Watson",
                    Email       = "emily.watson@underwritepro.com",
                    PhoneNumber = "8967231790",
                    Role        = "Compliance",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "Compliance",
                "Comp@Emily123!"),

                // ── 5. Operations ─────────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "Michael",
                    LastName    = "Torres",
                    Email       = "michael.torres@underwritepro.com",
                    PhoneNumber = "7856234765",
                    Role        = "Operations",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "Operations",
                "Ops@Michael123!"),

                // ── 6. PricingAnalyst ─────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "Priya",
                    LastName    = "Sharma",
                    Email       = "priya.sharma@underwritepro.com",
                    PhoneNumber = "9876543210",
                    Role        = "PricingAnalyst",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "PricingAnalyst",
                "Price@Priya123!"),

                // ── 7. UWAssistant ────────────────────────────────
                (new ApplicationUser
                {
                    Id          = Guid.NewGuid(),
                    FirstName   = "Rahul",
                    LastName    = "Verma",
                    Email       = "rahul.verma@underwritepro.com",
                    PhoneNumber = "8765432109",
                    Role        = "UWAssistant",
                    Status      = "Active",
                    CreatedAt   = DateTime.UtcNow
                },
                "UWAssistant",
                "Uwa@Rahul123!")
            };
        }
    }
}