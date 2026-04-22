using IdentityAndAccessManagement.DTOs;

namespace IdentityAndAccessManagement.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto);
        Task<UserDto> AssignRoleAsync(AssignRoleDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task LogoutAsync(LogoutDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}