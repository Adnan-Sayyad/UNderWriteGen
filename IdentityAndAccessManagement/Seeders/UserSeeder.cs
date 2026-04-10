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
                    FirstName = "Shiva",
                    LastName  = "Molige",
                    Email     = "moligeshiva@admin.com",
                    Role      = "Admin",
                    Status    = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                "Admin",
                "Admin@shiva2781"),

                // ── 2. Underwriter ────────────────────────────────
                (new ApplicationUser
                {
                    FirstName = "Sarah",
                    LastName  = "Mitchell",
                    Email     = "sarah.mitchell@underwritepro.com",
                    Role      = "Underwriter",
                    Status    = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                "Underwriter",
                "Uw@Sarah123!"),

                // ── 3. Agent ──────────────────────────────────────
                (new ApplicationUser
                {
                    FirstName = "James",
                    LastName  = "Carter",
                    Email     = "james.carter@underwritepro.com",
                    Role      = "Agent",
                    Status    = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                "Agent",
                "Agent@James123!"),

                // ── 4. Compliance ─────────────────────────────────
                (new ApplicationUser
                {
                    FirstName = "Emily",
                    LastName  = "Watson",
                    Email     = "emily.watson@underwritepro.com",
                    Role      = "Compliance",
                    Status    = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                "Compliance",
                "Comp@Emily123!"),

                // ── 5. Operations ─────────────────────────────────
                (new ApplicationUser
                {
                    FirstName = "Michael",
                    LastName  = "Torres",
                    Email     = "michael.torres@underwritepro.com",
                    Role      = "Operations",
                    Status    = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                "Operations",
                "Ops@Michael123!")
            };
        }
    }
}