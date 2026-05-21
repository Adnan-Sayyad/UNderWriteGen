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
        public DbSet<RiskScore> RiskScores { get; set; }
    }
}
