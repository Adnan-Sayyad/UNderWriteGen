namespace RulesScoringAndReferralMatrix.DTOs
{
    /// <summary>
    /// Generic paged result envelope shared by Rules / Matrix / Referrals / RiskScores list endpoints.
    /// Page and Size are 0-based and clamped to sensible bounds by callers.
    /// </summary>
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Content { get; set; } = [];
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalElements { get; set; }
        public int TotalPages { get; set; }
    }

    public static class PaginationHelpers
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize     = 200;

        public static (int page, int size) Normalize(int? page, int? size)
        {
            var p = Math.Max(0, page ?? 0);
            var s = size ?? DefaultPageSize;
            s = Math.Clamp(s, 1, MaxPageSize);
            return (p, s);
        }
    }
}
