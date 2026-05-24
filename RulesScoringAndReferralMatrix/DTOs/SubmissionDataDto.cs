namespace RulesScoringAndReferralMatrix.DTOs
{
    public class SubmissionDataDto
    {
        public Guid    SubmissionId       { get; set; }
        public string  ProductLine        { get; set; } = string.Empty;
        public decimal SumInsured         { get; set; }
        public int     PolicyTenureMonths { get; set; }
        public string  OccupationType     { get; set; } = string.Empty;
    }
}
