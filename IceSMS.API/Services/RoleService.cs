using Microsoft.EntityFrameworkCore;
using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Role;

namespace IceSMS.API.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleResponse>> GetRolesAsync(int tenantId)
    {
        var roles = await _context.Roles
            .Include(r => r.Permissions)
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        return roles.Select(r => MapToRoleResponse(r)).ToList();
    }

    public async Task<RoleResponse?> GetRoleByIdAsync(int roleId)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        return role == null ? null : MapToRoleResponse(role);
    }

    public async Task<Role> CreateRoleAsync(int tenantId, CreateRoleRequest request)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.TenantId == tenantId))
            throw new InvalidOperationException("Bu rol adı zaten kullanılıyor");

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            TenantId = tenantId,
            IsSystem = false
        };

        // İzinleri ekle
        if (request.Permissions.Any())
        {
            var permissions = await _context.Permissions
                .Where(p => request.Permissions.Contains(p.Name))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                role.Permissions.Add(permission);
            }
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return role;
    }

    public async Task<Role> UpdateRoleAsync(int roleId, UpdateRoleRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            throw new InvalidOperationException("Rol bulunamadı");

        if (role.IsSystem)
            throw new InvalidOperationException("Sistem rolleri düzenlenemez");

        if (request.Name != null && request.Name != role.Name)
        {
            if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.TenantId == role.TenantId))
                throw new InvalidOperationException("Bu rol adı zaten kullanılıyor");
            role.Name = request.Name;
        }

        if (request.Description != null)
            role.Description = request.Description;

        // İzinleri güncelle
        if (request.Permissions != null)
        {
            role.Permissions.Clear();
            var permissions = await _context.Permissions
                .Where(p => request.Permissions.Contains(p.Name))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                role.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();

        return role;
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return false;

        if (role.IsSystem)
            throw new InvalidOperationException("Sistem rolleri silinemez");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignPermissionsAsync(int roleId, List<string> permissions)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return false;

        if (role.IsSystem)
            throw new InvalidOperationException("Sistem rollerinin izinleri değiştirilemez");

        var permissionsToAdd = await _context.Permissions
            .Where(p => permissions.Contains(p.Name))
            .ToListAsync();

        foreach (var permission in permissionsToAdd)
        {
            if (!role.Permissions.Any(p => p.Id == permission.Id))
            {
                role.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokePermissionsAsync(int roleId, List<string> permissions)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return false;

        if (role.IsSystem)
            throw new InvalidOperationException("Sistem rollerinin izinleri değiştirilemez");

        var permissionsToRemove = role.Permissions.Where(p => permissions.Contains(p.Name)).ToList();
        foreach (var permission in permissionsToRemove)
        {
            role.Permissions.Remove(permission);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Select(p => p.Name)
            .ToListAsync();
    }

    private static RoleResponse MapToRoleResponse(Role role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            Permissions = role.Permissions.Select(p => p.Name).ToList()
        };
    }
} 