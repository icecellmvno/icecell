using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Role;

namespace IceSMS.API.Services;

public interface IRoleService
{
    Task<List<RoleResponse>> GetRolesAsync(int tenantId);
    Task<RoleResponse?> GetRoleByIdAsync(int roleId);
    Task<Role> CreateRoleAsync(int tenantId, CreateRoleRequest request);
    Task<Role> UpdateRoleAsync(int roleId, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(int roleId);
    Task<bool> AssignPermissionsAsync(int roleId, List<string> permissions);
    Task<bool> RevokePermissionsAsync(int roleId, List<string> permissions);
    Task<List<string>> GetAllPermissionsAsync();
} 