using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ComplianceAuditAndQA.Data
{
    // ── Used by EF CLI tools (migrations) ─────────────────────────
    public class ComplianceDbContextFactory
        : IDesignTimeDbContextFactory<ComplianceDbContext>
    {
        public ComplianceDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<ComplianceDbContext>()
                .UseSqlServer(config.GetConnectionString("DefaultConnection"))
                .Options;

            return new ComplianceDbContext(options);
        }
    }
}
