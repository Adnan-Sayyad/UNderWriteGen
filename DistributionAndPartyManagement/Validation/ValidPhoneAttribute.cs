using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DistributionAndPartyManagement.Validation
{
    /// <summary>
    /// Validates a phone number: digits only (spaces, dashes, parentheses, + stripped first),
    /// maximum 10 digits.
    /// </summary>
    public class ValidPhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is null || value is string s && string.IsNullOrWhiteSpace(s))
                return ValidationResult.Success;

            if (value is string phone)
            {
                var digitsOnly = Regex.Replace(phone, @"[\s\-\(\)\+]", "");

                if (!Regex.IsMatch(digitsOnly, @"^\d+$"))
                    return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} must contain only digits (spaces, dashes, and parentheses are allowed as separators).");

                if (digitsOnly.Length > 10)
                    return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} must not exceed 10 digits.");

                if (digitsOnly.Length < 7)
                    return new ValidationResult(ErrorMessage ?? $"{context.DisplayName} must be at least 7 digits.");
            }

            return ValidationResult.Success;
        }
    }
}
