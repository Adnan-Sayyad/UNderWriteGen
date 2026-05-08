using System.ComponentModel.DataAnnotations;

namespace DistributionAndPartyManagement.Validation
{
    /// <summary>
    /// Validates a date/datetime: must not be in the future and must be after 1 Jan 1900.
    /// </summary>
    public class ValidDOBAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is null)
                return ValidationResult.Success;

            DateTime date = value switch
            {
                DateTime dt => dt,
                DateTimeOffset dto => dto.DateTime,
                _ => DateTime.MinValue
            };

            if (date == DateTime.MinValue)
                return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} is not a valid date.");

            if (date.Date > DateTime.UtcNow.Date)
                return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} cannot be a future date.");

            if (date.Year < 1900)
                return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} must be on or after 1 January 1900.");

            return ValidationResult.Success;
        }
    }
}
