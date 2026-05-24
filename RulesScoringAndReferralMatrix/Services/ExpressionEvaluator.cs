using System.Text.Json;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Services
{
    /// <summary>
    /// Evaluates a UWRule's ExpressionJSON against real submission data.
    ///
    /// ExpressionJSON schema:
    ///   {
    ///     "logic": "AND" | "OR",
    ///     "conditions": [
    ///       { "field": "OccupationType", "operator": "equals",      "value": "Mining"   },
    ///       { "field": "SumInsured",     "operator": "greaterThan", "value": "5000000"  },
    ///       { "field": "ProductLine",    "operator": "equals",      "value": "Commercial" },
    ///       { "field": "PolicyTenureMonths", "operator": "greaterThanOrEqual", "value": "24" }
    ///     ]
    ///   }
    ///
    /// Supported fields:    OccupationType, SumInsured, ProductLine, PolicyTenureMonths
    /// Supported operators: equals, notEquals, greaterThan, lessThan,
    ///                      greaterThanOrEqual, lessThanOrEqual, contains
    /// </summary>
    public static class ExpressionEvaluator
    {
        public static bool Evaluate(string expressionJson, SubmissionDataDto submission)
        {
            if (string.IsNullOrWhiteSpace(expressionJson))
                return false;

            try
            {
                var doc = JsonSerializer.Deserialize<ExpressionDoc>(expressionJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (doc?.Conditions is null || doc.Conditions.Length == 0)
                    return false;

                var results = doc.Conditions.Select(c => EvaluateCondition(c, submission));

                return (doc.Logic ?? "AND").ToUpperInvariant() == "OR"
                    ? results.Any(r => r)
                    : results.All(r => r);
            }
            catch
            {
                // Malformed ExpressionJSON — treat rule as non-triggered
                return false;
            }
        }

        private static bool EvaluateCondition(ConditionDoc condition, SubmissionDataDto submission)
        {
            if (condition.Field is null || condition.Operator is null || condition.Value is null)
                return false;

            string fieldName = condition.Field.ToLowerInvariant();
            string op        = condition.Operator.ToLowerInvariant();
            string ruleVal   = condition.Value;

            // Numeric fields — compare as decimals
            if (fieldName is "suminsured" or "policytenuremonths")
            {
                decimal actual = fieldName == "suminsured"
                    ? submission.SumInsured
                    : submission.PolicyTenureMonths;

                if (!decimal.TryParse(ruleVal, out decimal target))
                    return false;

                return op switch
                {
                    "equals"             => actual == target,
                    "notequals"          => actual != target,
                    "greaterthan"        => actual >  target,
                    "lessthan"           => actual <  target,
                    "greaterthanorequal" => actual >= target,
                    "lessthanorequal"    => actual <= target,
                    _                    => false
                };
            }

            // String fields — compare case-insensitively
            string actualStr = fieldName switch
            {
                "occupationtype" => submission.OccupationType,
                "productline"    => submission.ProductLine,
                _                => string.Empty
            };

            return op switch
            {
                "equals"    => actualStr.Equals(ruleVal, StringComparison.OrdinalIgnoreCase),
                "notequals" => !actualStr.Equals(ruleVal, StringComparison.OrdinalIgnoreCase),
                "contains"  => actualStr.Contains(ruleVal, StringComparison.OrdinalIgnoreCase),
                _           => false
            };
        }

        private sealed class ExpressionDoc
        {
            public string?         Logic      { get; set; }
            public ConditionDoc[]? Conditions { get; set; }
        }

        private sealed class ConditionDoc
        {
            public string? Field    { get; set; }
            public string? Operator { get; set; }
            public string? Value    { get; set; }
        }
    }
}
