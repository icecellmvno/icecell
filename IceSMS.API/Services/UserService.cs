using Microsoft.EntityFrameworkCore;
using IceSMS.API.Data;
using IceSMS.API.Interfaces;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.User;

namespace IceSMS.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IProfileService _profileService;

    public UserService(ApplicationDbContext context, IProfileService profileService)
    {
        _context = context;
        _profileService = profileService;
    }

    public async Task<List<UserResponse>> GetUsersAsync(int tenantId, int page = 1, int pageSize = 10)
    {
        var users = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .Where(u => u.TenantId == tenantId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(u => MapToUserResponse(u)).ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? null : MapToUserResponse(user);
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            TenantId = request.TenantId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Rolleri ekle
        if (request.Roles.Any())
        {
            var roles = await _context.Roles
                .Where(r => request.Roles.Contains(r.Name))
                .ToListAsync();

            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Profil oluştur
        var profile = await _profileService.CreateProfileAsync(user.Id);
        if (request.PhoneNumber != null)
        {
            await _profileService.EnableSmsVerificationAsync(user.Id, request.PhoneNumber);
        }

        return user;
    }

    public async Task<User> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı");

        if (request.Username != null && request.Username != user.Username)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor");
            user.Username = request.Username;
        }

        if (request.Email != null && request.Email != user.Email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor");
            user.Email = request.Email;
        }

        if (request.Password != null)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if (request.FirstName != null)
            user.FirstName = request.FirstName;

        if (request.LastName != null)
            user.LastName = request.LastName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        // Rolleri güncelle
        if (request.Roles != null)
        {
            user.Roles.Clear();
            var roles = await _context.Roles
                .Where(r => request.Roles.Contains(r.Name))
                .ToListAsync();

            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }
        }

        await _context.SaveChangesAsync();

        // Profili güncelle
        if (request.PhoneNumber != null)
        {
            await _profileService.EnableSmsVerificationAsync(user.Id, request.PhoneNumber);
        }

        return user;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRolesAsync(int userId, List<string> roles)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        var rolesToAdd = await _context.Roles
            .Where(r => roles.Contains(r.Name))
            .ToListAsync();

        foreach (var role in rolesToAdd)
        {
            if (!user.Roles.Any(r => r.Id == role.Id))
            {
                user.Roles.Add(role);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeRolesAsync(int userId, List<string> roles)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        var rolesToRemove = user.Roles.Where(r => roles.Contains(r.Name)).ToList();
        foreach (var role in rolesToRemove)
        {
            user.Roles.Remove(role);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new InvalidOperationException("Mevcut şifre yanlış");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            TenantId = user.TenantId,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Permissions = user.Roles.SelectMany(r => r.Permissions.Select(p => p.Name)).Distinct().ToList()
        };
    }
} 