using IceSMS.API.Models;
using IceSMS.API.Models.Domain;

namespace IceSMS.API.Services;

public interface IProfileService
{
    Task<UserProfile> CreateProfileAsync(int userId);
    Task<UserProfile?> GetProfileAsync(int userId);
    Task<UserProfile> UpdateProfileAsync(int userId, UserProfile profile);
    
    Task<bool> EnableEmailVerificationAsync(int userId);
    Task<bool> DisableEmailVerificationAsync(int userId);
    
    Task<bool> EnableSmsVerificationAsync(int userId, string phoneNumber);
    Task<bool> DisableSmsVerificationAsync(int userId);
    Task<bool> VerifyPhoneNumberAsync(int userId, string code);
    
    Task<(string SecretKey, string QrCodeUri)> EnableGoogleAuthAsync(int userId, string email);
    Task<bool> DisableGoogleAuthAsync(int userId);
    Task<bool> VerifyGoogleAuthSetupAsync(int userId, string code);
    
    Task<bool> UpdateLastLoginAsync(int userId, string ipAddress);
} 