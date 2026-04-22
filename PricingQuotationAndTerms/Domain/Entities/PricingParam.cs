namespace PricingQuotationAndTerms.Domain.Entities;

public class PricingParam
{
    private PricingParam() { }

    public PricingParam(
        string   productLine,
        string   paramName,
        decimal  value,
        string   description,
        DateTime effectiveFrom,
        DateTime? effectiveTo = null)
    {
        Id            = Guid.NewGuid();
        ProductLine   = productLine;
        ParamName     = paramName;
        Value         = value;
        Description   = description;
        EffectiveFrom = effectiveFrom;
        EffectiveTo   = effectiveTo;
        IsActive      = true;
        CreatedAt     = DateTime.UtcNow;
    }

    public Guid      Id            { get; private set; }
    public string    ProductLine   { get; private set; } = string.Empty;
    public string    ParamName     { get; private set; } = string.Empty;
    public decimal   Value         { get; private set; }
    public string    Description   { get; private set; } = string.Empty;
    public DateTime  EffectiveFrom { get; private set; }  // When this rate became active
    public DateTime? EffectiveTo   { get; private set; }  // null = no expiry
    public bool      IsActive      { get; private set; }
    public DateTime  CreatedAt     { get; private set; }
    public DateTime? UpdatedAt     { get; private set; }

    public void UpdateValue(decimal newValue, string newDescription)
    {
        if (newValue < 0)
            throw new ArgumentException("Value cannot be negative.");
        Value       = newValue;
        Description = newDescription;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void SetEffectivePeriod(DateTime from, DateTime? to)
    {
        if (to.HasValue && to.Value <= from)
            throw new ArgumentException("EffectiveTo must be after EffectiveFrom.");
        EffectiveFrom = from;
        EffectiveTo   = to;
        UpdatedAt     = DateTime.UtcNow;
    }

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public bool IsEffectiveOn(DateTime date) =>
        IsActive && EffectiveFrom <= date && (EffectiveTo == null || EffectiveTo >= date);
}
