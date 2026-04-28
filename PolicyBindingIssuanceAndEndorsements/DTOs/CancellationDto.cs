namespace PolicyBindingIssuanceAndEndorsements.DTOs
{
    public class CreateCancellationDto
    {
        public Guid PolicyID { get; set; }
        public string CancelReason { get; set; } = string.Empty;
        public DateTime CancelDate { get; set; }
        public decimal RefundPremium { get; set; }
    }

    public class UpdateCancellationStatusDto
    {
        /// <summary>Requested / Approved / Posted</summary>
        public string Status { get; set; } = string.Empty;
    }
}
