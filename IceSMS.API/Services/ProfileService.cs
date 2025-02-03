using IceSMS.API.Data;
using IceSMS.API.Models;
using Microsoft.EntityFrameworkCore;
using IceSMS.API.Models.Domain;

namespace IceSMS.API.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IGoogleAuthService _googleAuthService;
 
    public ProfileService(
        ApplicationDbContext context,
        IGoogleAuthService googleAuthService
    )
    {
        _context = context;
        _googleAuthService = googleAuthService;

    }

    public async Task<UserProfile?> GetProfileAsync(int userId)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserProfile> CreateProfileAsync(int userId)
    {
        var profile = new UserProfile
        {
            UserId = userId,
            IsPhoneVerified = false,
            IsSmsVerificationEnabled = false,
            IsEmailVerificationEnabled = false,
            IsGoogleAuthEnabled = false
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<UserProfile> UpdateProfileAsync(int userId, UserProfile profile)
    {
        var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existingProfile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        existingProfile.PhoneNumber = profile.PhoneNumber;
        existingProfile.IsPhoneVerified = profile.IsPhoneVerified;
        existingProfile.IsSmsVerificationEnabled = profile.IsSmsVerificationEnabled;
        existingProfile.IsEmailVerificationEnabled = profile.IsEmailVerificationEnabled;
        existingProfile.IsGoogleAuthEnabled = profile.IsGoogleAuthEnabled;
        existingProfile.GoogleAuthSecretKey = profile.GoogleAuthSecretKey;

        await _context.SaveChangesAsync();

        return existingProfile;
    }

    public async Task<bool> EnableEmailVerificationAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        profile.IsEmailVerificationEnabled = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableEmailVerificationAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        profile.IsEmailVerificationEnabled = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableSmsVerificationAsync(int userId, string phoneNumber)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        profile.PhoneNumber = phoneNumber;
        profile.IsPhoneVerified = false;
        profile.IsSmsVerificationEnabled = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableSmsVerificationAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        profile.IsSmsVerificationEnabled = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyPhoneNumberAsync(int userId, string code)
    {
        var profile = await GetProfileAsync(userId);
        if (profile == null || profile.PhoneNumber == null) return false;

        // Dummy implementasyon - gerçekte kodu Redis'te saklayıp kontrol etmek gerekir
        profile.IsPhoneVerified = true;
        await UpdateProfileAsync(userId, profile);
        return true;
    }

    public async Task<(string SecretKey, string QrCodeUri)> EnableGoogleAuthAsync(int userId, string email)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        var (secretKey, qrCodeUri) = _googleAuthService.GenerateSetupInfo(email);

        profile.GoogleAuthSecretKey = secretKey;
        profile.IsGoogleAuthEnabled = true;

        await _context.SaveChangesAsync();

        return (secretKey, qrCodeUri);
    }

    public async Task<bool> DisableGoogleAuthAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        profile.IsGoogleAuthEnabled = false;
        profile.GoogleAuthSecretKey = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyGoogleAuthSetupAsync(int userId, string code)
    {
        var profile = await GetProfileAsync(userId);
        if (profile == null || profile.GoogleAuthSecretKey == null) return false;

        var isValid = _googleAuthService.ValidateCode(profile.GoogleAuthSecretKey, code);
        if (isValid)
        {
            profile.IsGoogleAuthEnabled = true;
            await UpdateProfileAsync(userId, profile);
        }

        return isValid;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId, string ipAddress)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new InvalidOperationException("Profil bulunamadı");

        profile.LastLoginAt = DateTime.UtcNow;
        profile.LastLoginIp = ipAddress;

        await _context.SaveChangesAsync();
        return true;
    }
} 