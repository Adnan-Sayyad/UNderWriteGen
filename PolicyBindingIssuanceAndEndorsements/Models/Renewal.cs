namespace PolicyBindingIssuanceAndEndorsements.Models
{
    public class Renewal
    {
        public Guid RenewalID { get; set; } = Guid.NewGuid();
        public Guid PolicyID { get; set; }
        public string RenewalOfferJSON { get; set; } = "{}";
        public DateTime OfferedDate { get; set; } = DateTime.UtcNow;

        /// <summary>Offered / Accepted / Declined</summary>
        public string Status { get; set; } = "Offered";
    }
}
