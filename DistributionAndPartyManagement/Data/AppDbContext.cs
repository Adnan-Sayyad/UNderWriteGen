using Microsoft.EntityFrameworkCore;
using DistributionAndPartyManagement.Models.Entities;

namespace DistributionAndPartyManagement.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Agent> Agents => Set<Agent>();
		public DbSet<CustomerParty> CustomerParties => Set<CustomerParty>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// ---- Agent config ----
			modelBuilder.Entity<Agent>(entity =>
			{
				entity.ToTable("Agent");
				entity.HasIndex(a => a.ProducerCode).IsUnique();
				entity.HasIndex(a => a.Name);
				entity.HasIndex(a => a.Region);
				entity.HasIndex(a => a.Status);
			});

			// ---- CustomerParty config ----
			modelBuilder.Entity<CustomerParty>(entity =>
			{
				entity.ToTable("CustomerParty");
				entity.HasIndex(c => c.Name);
				entity.HasIndex(c => c.PartyType);
				entity.HasIndex(c => c.Segment);
				entity.HasIndex(c => c.Status);
			});

		}
	}
}
