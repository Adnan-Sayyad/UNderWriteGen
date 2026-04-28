namespace PolicyBindingIssuanceAndEndorsements.Models
{
    public class Endorsement
    {
        public Guid EndorsementID { get; set; } = Guid.NewGuid();
        public Guid PolicyID { get; set; }

        /// <summary>MidTermChange / Address / Limit / Deductible / Beneficiary</summary>
        public string EndorsementType { get; set; } = string.Empty;

        public string ChangesJSON { get; set; } = "{}";
        public DateTime EffectiveDate { get; set; }
        public decimal PremiumDelta { get; set; }

        /// <summary>Proposed / Approved / Posted</summary>
        public string Status { get; set; } = "Proposed";
    }
}
