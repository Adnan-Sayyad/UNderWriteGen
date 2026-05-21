using Microsoft.EntityFrameworkCore;
using PricingQuotationAndTerms.Domain.Entities;

namespace PricingQuotationAndTerms.Infrastructure.Data;

public class QuoteDbContext : DbContext
{
    public QuoteDbContext(DbContextOptions<QuoteDbContext> options) : base(options) { }

    public DbSet<Quote>        Quotes        => Set<Quote>();
    public DbSet<PricingParam> PricingParams => Set<PricingParam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Quote ────────────────────────────────────────────────────
        modelBuilder.Entity<Quote>(e =>
        {
            e.ToTable("Quotes");
            e.HasKey(q => q.Id);
            e.Property(q => q.Id).ValueGeneratedNever();
            e.Property(q => q.SubmissionId).IsRequired();
            e.Property(q => q.BasePremium).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(q => q.TotalPremium).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(q => q.LoadingsJson).HasColumnType("nvarchar(max)").HasDefaultValue(string.Empty);
            e.Property(q => q.DiscountsJson).HasColumnType("nvarchar(max)").HasDefaultValue(string.Empty);
            e.Property(q => q.TaxesJson).HasColumnType("nvarchar(max)").HasDefaultValue(string.Empty);
            e.Property(q => q.TermsJson).HasColumnType("nvarchar(max)").HasDefaultValue(string.Empty);
            e.Property(q => q.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(q => q.VersionNo).IsRequired();
            e.Property(q => q.ValidUntil).IsRequired();
            e.Property(q => q.CreatedAt).IsRequired();
            e.HasIndex(q => q.SubmissionId).HasDatabaseName("IX_Quotes_SubmissionId");
            e.HasIndex(q => q.Status).HasDatabaseName("IX_Quotes_Status");
        });

        // ── PricingParam ─────────────────────────────────────────────
        modelBuilder.Entity<PricingParam>(e =>
        {
            e.ToTable("PricingParams");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();
            e.Property(p => p.ProductLine).HasMaxLength(50).IsRequired();
            e.Property(p => p.ParamName).HasMaxLength(100).IsRequired();
            e.Property(p => p.Value).HasColumnType("decimal(18,6)").IsRequired();
            e.Property(p => p.Description).HasMaxLength(250).IsRequired();
            e.Property(p => p.EffectiveFrom).IsRequired();
            e.Property(p => p.EffectiveTo);
            e.Property(p => p.IsActive).IsRequired();
            e.Property(p => p.CreatedAt).IsRequired();
            e.HasIndex(p => new { p.ProductLine, p.ParamName, p.IsActive })
             .HasDatabaseName("IX_PricingParams_ProductLine_ParamName_Active");
        });

        SeedPricingParams(modelBuilder);
    }

    private static void SeedPricingParams(ModelBuilder b)
    {
        var epoch = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var seeds = new[]
        {
            // ── Base rates per product line ───────────────────────
            S("Motor",      "BaseRate",                  0.025m, "Motor: 2.5% per annum",            epoch),
            S("Health",     "BaseRate",                  0.035m, "Health: 3.5% per annum",            epoch),
            S("Property",   "BaseRate",                  0.015m, "Property: 1.5% per annum",          epoch),
            S("Life",       "BaseRate",                  0.020m, "Life: 2.0% per annum",              epoch),
            S("PnC",        "BaseRate",                  0.022m, "PnC: 2.2% per annum",               epoch),
            S("Commercial", "BaseRate",                  0.018m, "Commercial: 1.8% per annum",        epoch),
            // ── Min premium per product line (overrides Global floor) ──
            S("PnC",        "MinimumPremium",            750m,   "PnC floor premium: ₹750",          epoch),
            S("Commercial", "MinimumPremium",           1500m,   "Commercial floor premium: ₹1500",  epoch),

            // ── Risk loadings ─────────────────────────────────────
            S("Global",   "RiskLoading_Medium",          0.10m,  "+10% for Medium risk band",        epoch),
            S("Global",   "RiskLoading_High",            0.25m,  "+25% for High risk band",          epoch),

            // ── Occupation loadings (Health & Life) ───────────────
            S("Global",   "OccupationLoad_Mining",       0.30m,  "Mining: +30%",                     epoch),
            S("Global",   "OccupationLoad_Construction", 0.20m,  "Construction: +20%",               epoch),
            S("Global",   "OccupationLoad_Agriculture",  0.15m,  "Agriculture: +15%",                epoch),
            S("Global",   "OccupationLoad_Transport",    0.12m,  "Transport: +12%",                  epoch),
            S("Global",   "OccupationLoad_IT",           0.00m,  "IT: 0% (low risk)",                epoch),
            S("Global",   "OccupationLoad_Banking",      0.00m,  "Banking: 0% (low risk)",           epoch),
            S("Global",   "OccupationLoad_Education",    0.00m,  "Education: 0% (low risk)",         epoch),

            // ── Tenure discounts ──────────────────────────────────
            S("Global",   "TenureDiscount_12m",          0.03m,  "3% discount for 12-month policy",  epoch),
            S("Global",   "TenureDiscount_24m",          0.05m,  "5% discount for 24-month policy",  epoch),
            S("Global",   "TenureDiscount_36m",          0.07m,  "7% discount for 36-month policy",  epoch),

            // ── Agent discount ────────────────────────────────────
            S("Global",   "AgentDiscount_Preferred",     0.02m,  "2% discount via preferred agent",  epoch),

            // ── Loyalty & Tax & Floor ─────────────────────────────
            S("Global",   "LoyaltyDiscount",             0.05m,  "5% loyalty discount for renewals", epoch),
            S("Global",   "GstRate",                     0.18m,  "GST: 18% on adjusted premium",     epoch),
            S("Global",   "MinimumPremium",              500m,   "Floor premium: ₹500",              epoch),
        };

        b.Entity<PricingParam>().HasData(seeds);
    }

    private static object S(
        string productLine, string paramName, decimal value,
        string description, DateTime effectiveFrom) => new
    {
        Id            = new Guid(System.Security.Cryptography.MD5.HashData(
                            System.Text.Encoding.UTF8.GetBytes($"{productLine}_{paramName}"))),
        ProductLine   = productLine,
        ParamName     = paramName,
        Value         = value,
        Description   = description,
        EffectiveFrom = effectiveFrom,
        EffectiveTo   = (DateTime?)null,
        IsActive      = true,
        CreatedAt     = effectiveFrom,
        UpdatedAt     = (DateTime?)null
    };
}
