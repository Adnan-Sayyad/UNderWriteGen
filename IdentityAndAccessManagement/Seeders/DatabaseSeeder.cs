using IdentityAndAccessManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndAccessManagement.Seeders
{
    public class DatabaseSeeder
    {
        private readonly RoleSeeder _roleSeeder;
        private readonly UserSeeder _userSeeder;
        private readonly ApplicationUserDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            RoleSeeder roleSeeder,
            UserSeeder userSeeder,
            ApplicationUserDbContext context,
            ILogger<DatabaseSeeder> logger)
        {
            _roleSeeder = roleSeeder;
            _userSeeder = userSeeder;
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                // ── Step 1: Apply pending migrations ──────────────
                await MigrateDatabaseAsync();

                // ── Step 2: Seed roles ────────────────────────────
                // Roles must exist before users are created
                _logger.LogInformation("Seeding roles...");
                await _roleSeeder.SeedAsync();
                _logger.LogInformation("Roles seeding completed.");

                // ── Step 3: Seed users ────────────────────────────
                // Users depend on roles being seeded first
                _logger.LogInformation("Seeding users...");
                await _userSeeder.SeedAsync();
                _logger.LogInformation("Users seeding completed.");

                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        // ── Private: Apply Migrations ─────────────────────────────
        private async Task MigrateDatabaseAsync()
        {
            try
            {
                var pendingMigrations = await _context.Database
                    .GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation(
                        "{Count} pending migration(s) found. Applying...",
                        pendingMigrations.Count());

                    await _context.Database.MigrateAsync();

                    _logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    _logger.LogInformation(
                        "No pending migrations — database is up to date.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying migrations.");
                throw;
            }
        }
    }
}