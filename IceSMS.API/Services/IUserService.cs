using IceSMS.API.Models.Domain;
using IceSMS.API.Models.User;

namespace IceSMS.API.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync(int tenantId, int page = 1, int pageSize = 10);
    Task<UserResponse?> GetUserByIdAsync(int userId);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> AssignRolesAsync(int userId, List<string> roles);
    Task<bool> RevokeRolesAsync(int userId, List<string> roles);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(int userId, string newPassword);
    Task<bool> ToggleUserStatusAsync(int userId, bool isActive);
} 