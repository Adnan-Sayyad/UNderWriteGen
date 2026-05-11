using System.ComponentModel.DataAnnotations;

namespace DistributionAndPartyManagement.Validation
{
    /// <summary>
    /// Ensures the string value contains no numeric digits.
    /// </summary>
    public class NoDigitsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is string str && str.Any(char.IsDigit))
                return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} must not contain numbers.");

            return ValidationResult.Success;
        }
    }
}
