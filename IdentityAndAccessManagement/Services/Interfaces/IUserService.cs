using IdentityAndAccessManagement.DTOs;

namespace IdentityAndAccessManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(Guid adminId);
        Task<UserDto> GetUserByIdAsync(Guid adminId, Guid userId);
        Task<UserDto> UpdateUserAsync(Guid adminId, Guid userId, UpdateUserDto dto);
        Task UpdateUserStatusAsync(Guid adminId, Guid userId, UpdateUserStatusDto dto);
        Task DeleteUserAsync(Guid adminId, Guid userId);
    }
}
