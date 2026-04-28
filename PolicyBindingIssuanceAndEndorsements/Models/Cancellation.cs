namespace PolicyBindingIssuanceAndEndorsements.Models
{
    public class Cancellation
    {
        public Guid CancellationID { get; set; } = Guid.NewGuid();
        public Guid PolicyID { get; set; }
        public string CancelReason { get; set; } = string.Empty;
        public DateTime CancelDate { get; set; }
        public decimal RefundPremium { get; set; }

        /// <summary>Requested / Approved / Posted</summary>
        public string Status { get; set; } = "Requested";
    }
}
