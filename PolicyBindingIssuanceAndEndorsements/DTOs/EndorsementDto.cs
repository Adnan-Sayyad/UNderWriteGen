namespace PolicyBindingIssuanceAndEndorsements.DTOs
{
    public class CreateEndorsementDto
    {
        public Guid PolicyID { get; set; }

        /// <summary>MidTermChange / Address / Limit / Deductible / Beneficiary</summary>
        public string EndorsementType { get; set; } = string.Empty;

        public string ChangesJSON { get; set; } = "{}";
        public DateTime EffectiveDate { get; set; }
        public decimal PremiumDelta { get; set; }
    }

    public class UpdateEndorsementDto
    {
        public string EndorsementType { get; set; } = string.Empty;
        public string ChangesJSON { get; set; } = "{}";
        public DateTime EffectiveDate { get; set; }
        public decimal PremiumDelta { get; set; }
    }

    public class UpdateEndorsementStatusDto
    {
        /// <summary>Proposed / Approved / Posted</summary>
        public string Status { get; set; } = string.Empty;
    }
}
