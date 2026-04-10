using IdentityAndAccessManagement.DTOs;

namespace IdentityAndAccessManagement.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task LogoutAsync(string userId);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}