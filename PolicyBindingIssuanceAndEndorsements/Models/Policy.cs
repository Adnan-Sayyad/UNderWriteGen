namespace PolicyBindingIssuanceAndEndorsements.Models
{
    public class Policy
    {
        public Guid PolicyID { get; set; } = Guid.NewGuid();
        public Guid SubmissionID { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;

        /// <summary>Life / Health / PnC / Commercial</summary>
        public string ProductLine { get; set; } = string.Empty;

        public string CoverageJSON { get; set; } = "{}";
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        /// <summary>Active / Cancelled / Expired</summary>
        public string Status { get; set; } = "Active";
    }
}
