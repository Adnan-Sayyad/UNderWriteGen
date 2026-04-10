using IdentityAndAccessManagement.DTOs;

namespace IdentityAndAccessManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto dto);
        Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto);
        Task DeleteUserAsync(Guid userId);
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
        Task UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto dto);
    }
}