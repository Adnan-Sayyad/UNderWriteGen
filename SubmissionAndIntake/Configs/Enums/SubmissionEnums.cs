namespace SubmissionAndIntake.Configs.Enums
{
    public enum ProductLine
    {
        Life,
        Health,
        PnC,
        Commercial
    }

    public enum SubmissionStatus
    {
        Draft          = 0,
        IntakeComplete = 1,
        UnderReview    = 2,
        Quoted         = 3,
        Declined       = 4,
        Expired        = 5,
        Approved       = 6,   // UW approved → ready for pricing
        PolicyBound    = 7,   // Policy issued → pending compliance
        Issued         = 8,   // Compliance signed off → policy in force
    }
}
