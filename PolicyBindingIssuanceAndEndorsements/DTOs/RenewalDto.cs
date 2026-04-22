namespace PolicyBindingIssuanceAndEndorsements.DTOs
{
    public class CreateRenewalDto
    {
        public Guid PolicyID { get; set; }
        public string RenewalOfferJSON { get; set; } = "{}";
    }

    public class UpdateRenewalStatusDto
    {
        /// <summary>Offered / Accepted / Declined</summary>
        public string Status { get; set; } = string.Empty;
    }
}
