using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Data
{
    public class RulesScoringAndReferralMatrixDbContext : DbContext
    {
        public RulesScoringAndReferralMatrixDbContext(DbContextOptions<RulesScoringAndReferralMatrixDbContext> options)
            : base(options)
        {
        }
        public DbSet<UWRule> Rules { get; set; }
        public DbSet<RiskScore> Referrals { get; set; }
        public DbSet<ReferralMatrix> ReferralMatrices { get; set; }
        public DbSet<Referral> AuditLogs { get; set; }
    }
}
