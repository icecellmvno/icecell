using IceSMS.API.Models.Auth;
using IceSMS.API.Models.Domain;

namespace IceSMS.API.Interfaces;

public interface IAuthService
{
    // Normal kullanıcı işlemleri
    Task<User> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(string sessionId);
    
    // 2FA işlemleri
    Task<LoginResponse> VerifyTwoFactorAsync(TwoFactorRequest request);
    Task<bool> SendTwoFactorCodeAsync(string sessionId, TwoFactorType type);
    Task<(string SecretKey, string QrCodeUri)> SetupGoogleAuthAsync(int userId, string email);
    
    // Email doğrulama
    Task<bool> SendEmailVerificationAsync(int userId, string email);
    Task<bool> VerifyEmailAsync(int userId, string token);
    
    // API key işlemleri
    Task<ApiKeyResponse> ValidateApiKeyAsync(string apiKey);
    Task<string> GenerateApiKeyAsync(int userId, int tenantId);
    Task<bool> RevokeApiKeyAsync(string apiKey);
    
    // Token işlemleri
    Task<string> GenerateAccessTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
} 