using Microsoft.AspNetCore.Identity;

namespace IdentityAndAccessManagement.Seeders
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<RoleSeeder> _logger;

        public RoleSeeder(
            RoleManager<IdentityRole<Guid>> roleManager,
            ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var roles = new[]
            {
                "Admin",          // Full system access
                "Agent",          // Submit proposals, retrieve quotes
                "Underwriter",    // Assess risk, approve/decline
                "UWAssistant",    // Collect info, manage checklist
                "PricingAnalyst", // Maintain pricing parameters
                "Compliance",     // Review audit trails, exceptions
                "Operations"      // Bind, issue policies, endorsements
            };

            foreach (var role in roles)
            {
                await SeedRoleAsync(role);
            }
        }

        // ── Private: Seed Single Role ─────────────────────────────
        private async Task SeedRoleAsync(string roleName)
        {
            try
            {
                var exists = await _roleManager.RoleExistsAsync(roleName);

                if (exists)
                {
                    _logger.LogInformation(
                        "Role '{Role}' already exists — skipping.", roleName);
                    return;
                }

                var role = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName
                };

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                    _logger.LogInformation(
                        "Role '{Role}' created successfully.", roleName);
                else
                    _logger.LogError(
                        "Failed to create role '{Role}': {Errors}",
                        roleName,
                        string.Join(", ", result.Errors
                            .Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception while seeding role '{Role}'.", roleName);
                throw;
            }
        }
    }
}