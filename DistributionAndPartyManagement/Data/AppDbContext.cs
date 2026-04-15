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

			// ---- Seed Agents ----
			modelBuilder.Entity<Agent>().HasData(
				new Agent
				{
					AgentID = "AGT-20250101-0001",
					Name = "Rajesh Kumar",
					ProducerCode = "MUM-LIF-001",
					ContactInfo = "Mumbai Office, Andheri West",
					Region = "Mumbai",
					Status = "Active"
				},
				new Agent
				{
					AgentID = "AGT-20250101-0002",
					Name = "Priya Sharma",
					ProducerCode = "DEL-PNC-002",
					ContactInfo = "Delhi Office, Connaught Place",
					Region = "Delhi",
					Status = "Active"
				}
			);

			// ---- Seed Customers ----
			modelBuilder.Entity<CustomerParty>().HasData(
				new CustomerParty
				{
					PartyID = "PTY-20250101-0001",
					PartyType = "Individual",
					Name = "Amit Patel",
					DOBIncorporation = new DateTime(1985, 6, 15),
					ContactInfo = "Pune, Maharashtra",
					Segment = "Retail",
					Status = "Active"
				},
				new CustomerParty
				{
					PartyID = "PTY-20250101-0002",
					PartyType = "Organization",
					Name = "TechCorp Solutions Pvt Ltd",
					DOBIncorporation = new DateTime(2010, 3, 20),
					ContactInfo = "Bangalore, Karnataka",
					Segment = "SME",
					Status = "Active"
				}
			);
		}
	}
}
