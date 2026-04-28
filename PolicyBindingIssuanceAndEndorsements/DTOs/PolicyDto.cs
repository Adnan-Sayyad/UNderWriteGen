namespace PolicyBindingIssuanceAndEndorsements.DTOs
{
    public class CreatePolicyDto
    {
        public Guid SubmissionID { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;

        /// <summary>Life / Health / PnC / Commercial</summary>
        public string ProductLine { get; set; } = string.Empty;

        public string CoverageJSON { get; set; } = "{}";
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class UpdatePolicyDto
    {
        public string ProductLine { get; set; } = string.Empty;
        public string CoverageJSON { get; set; } = "{}";
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class UpdatePolicyStatusDto
    {
        /// <summary>Active / Cancelled / Expired</summary>
        public string Status { get; set; } = string.Empty;
    }
}
