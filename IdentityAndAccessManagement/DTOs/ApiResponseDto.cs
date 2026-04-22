namespace IdentityAndAccessManagement.DTOs
{
    // ── Generic success / failure wrapper ─────────────────────────
    public class ApiResponseDto<T>
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T?     Data    { get; set; }

        // ── Factory helpers ───────────────────────────────────────
        public static ApiResponseDto<T> Ok(string message, T data) => new()
        {
            Success = true,
            Message = message,
            Data    = data
        };

        public static ApiResponseDto<T> Fail(string message) => new()
        {
            Success = false,
            Message = message,
            Data    = default
        };
    }

    // ── Non-generic version for responses with no data payload ────
    public class ApiResponseDto
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ApiResponseDto Ok(string message)   => new() { Success = true,  Message = message };
        public static ApiResponseDto Fail(string message) => new() { Success = false, Message = message };
    }
}
