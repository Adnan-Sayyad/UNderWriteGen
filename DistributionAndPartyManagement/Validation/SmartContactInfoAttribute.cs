using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DistributionAndPartyManagement.Validation
{
    /// <summary>
    /// Validates ContactInfo as either:
    ///   - A 10-digit mobile number (digits only, must not start with 0), or
    ///   - A valid email address (must contain @ with proper structure).
    /// Both formats are accepted; the format is auto-detected from the input.
    /// </summary>
    public class SmartContactInfoAttribute : ValidationAttribute
    {
        private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);
        private static readonly EmailAddressAttribute EmailValidator = new();

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is null || value is string s && string.IsNullOrWhiteSpace(s))
                return ValidationResult.Success;

            var input = ((string)value).Trim();

            // Strip common phone separators to get the raw digit string
            var stripped = Regex.Replace(input, @"[\s\-\(\)\+]", "");

            // --- Phone path: input consists only of digits (after stripping separators) ---
            if (DigitsOnly.IsMatch(stripped))
            {
                if (stripped.Length != 10)
                    return new ValidationResult(
                        $"You entered a phone number '{input}' — it must be exactly 10 digits (you entered {stripped.Length}).");

                if (stripped[0] == '0')
                    return new ValidationResult(
                        $"You entered a phone number '{input}' — it must not start with 0.");

                return ValidationResult.Success;
            }

            // --- Email path: input contains @ ---
            if (input.Contains('@'))
            {
                if (!EmailValidator.IsValid(input))
                    return new ValidationResult(
                        $"You entered an email address '{input}' — the format is invalid. Expected format: user@example.com");

                return ValidationResult.Success;
            }

            // --- Neither ---
            return new ValidationResult(
                $"ContactInfo '{input}' is not recognised. Please enter either a 10-digit mobile number (e.g. 9876543210) or a valid email address (e.g. user@example.com).");
        }
    }
}
