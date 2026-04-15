using System.Reflection.Emit;
using ReportingAndPortfolioAnalytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace ReportingAndPortfolioAnalytics.Persistence;

public class ReportingDbContext : DbContext
{
	public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
		: base(options) { }

	public DbSet<UWReport> UWReports => Set<UWReport>();

	protected override void OnModelCreating(ModelBuilder mb)
	{
		mb.Entity<UWReport>(e =>
		{
			// Match spec column names exactly
			e.HasKey(r => r.ReportID);
			e.Property(r => r.ReportID).HasColumnName("ReportID");

			e.Property(r => r.Scope)
				.HasColumnName("Scope")
				.HasMaxLength(50)
				.IsRequired();

			e.Property(r => r.ScopeValue)
				.HasMaxLength(100)
				.IsRequired();

			// MetricsJSON stores the full ReportMetrics object as JSON text
			e.Property(r => r.MetricsJSON)
				.HasColumnName("Metrics")       // column is named "Metrics" in the spec
				.HasColumnType("nvarchar(max)")
				.IsRequired();

			e.Property(r => r.GeneratedDate)
				.HasColumnName("GeneratedDate");

			e.Property(r => r.GeneratedBy)
				.HasMaxLength(100);

			e.Property(r => r.PeriodStart).IsRequired(false);
			e.Property(r => r.PeriodEnd).IsRequired(false);

			// Indexes for fast metric queries
			e.HasIndex(r => r.Scope);
			e.HasIndex(r => new { r.Scope, r.ScopeValue });
			e.HasIndex(r => r.GeneratedDate);
		});
	}
}