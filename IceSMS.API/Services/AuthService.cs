using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Auth;

namespace IceSMS.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ISessionService _sessionService;
    private readonly IEmailService _emailService;
 
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IProfileService _profileService;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ISessionService sessionService,
        IEmailService emailService,
   
        IGoogleAuthService googleAuthService,
        IProfileService profileService)
    {
        _context = context;
        _configuration = configuration;
        _sessionService = sessionService;
        _emailService = emailService;
  
        _googleAuthService = googleAuthService;
        _profileService = profileService;
    }

    public async Task<User> RegisterAsync(RegisterRequest request)
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
            LastName = request.LastName
        };

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

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Geçersiz kullanıcı adı veya şifre");

        if (!user.IsActive)
            throw new InvalidOperationException("Hesabınız aktif değil");

        // Profil kontrolü
        var profile = await _profileService.GetProfileAsync(user.Id);
        if (profile == null)
            throw new InvalidOperationException("Kullanıcı profili bulunamadı");

        // Session oluştur
        var session = await _sessionService.CreateSessionAsync(
            user.Id,
            user.TenantId,
            ipAddress,
            userAgent,
            profile.IsEmailVerificationEnabled || profile.IsSmsVerificationEnabled || profile.IsGoogleAuthEnabled
        );

        // Access token oluştur
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync();

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            SessionId = session.Id,
            RequiresEmailVerification = profile.IsEmailVerificationEnabled && !session.IsEmailVerified,
            Requires2FA = session.Is2FARequired && !session.Is2FACompleted
        };

        // 2FA gerekiyorsa tipini belirle
        if (response.Requires2FA)
        {
            if (profile.IsGoogleAuthEnabled)
            {
                response.TwoFactorType = TwoFactorType.GoogleAuth;
            }
            else if (profile.IsSmsVerificationEnabled && profile.IsPhoneVerified)
            {
                response.TwoFactorType = TwoFactorType.Sms;
                await SendTwoFactorCodeAsync(session.Id, TwoFactorType.Sms);
            }
            else if (profile.IsEmailVerificationEnabled)
            {
                response.TwoFactorType = TwoFactorType.Email;
                await SendTwoFactorCodeAsync(session.Id, TwoFactorType.Email);
            }
        }

        // Son giriş bilgilerini güncelle
        await _profileService.UpdateLastLoginAsync(user.Id, ipAddress);

        return response;
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionId);
        if (session == null)
            throw new InvalidOperationException("Geçersiz oturum");

        var isValid = await _sessionService.ValidateRefreshTokenAsync(request.SessionId, request.RefreshToken);
        if (!isValid)
            throw new InvalidOperationException("Geçersiz refresh token");

        var user = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == session.UserId);

        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Kullanıcı bulunamadı veya aktif değil");

        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            SessionId = session.Id,
            RequiresEmailVerification = false,
            Requires2FA = false
        };
    }

    public async Task<bool> LogoutAsync(string sessionId)
    {
        return await _sessionService.DeleteSessionAsync(sessionId);
    }

    public async Task<LoginResponse> VerifyTwoFactorAsync(TwoFactorRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionId);
        if (session == null)
            throw new InvalidOperationException("Geçersiz oturum");

        var user = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == session.UserId);

        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Kullanıcı bulunamadı veya aktif değil");

        var profile = await _profileService.GetProfileAsync(user.Id);
        if (profile == null)
            throw new InvalidOperationException("Kullanıcı profili bulunamadı");

        bool isValid = false;

        switch (request.Type)
        {
            case TwoFactorType.GoogleAuth:
                if (!profile.IsGoogleAuthEnabled || profile.GoogleAuthSecretKey == null)
                    throw new InvalidOperationException("Google Authenticator aktif değil");
                isValid = _googleAuthService.ValidateCode(profile.GoogleAuthSecretKey, request.Code);
                break;

            case TwoFactorType.Sms:
                if (!profile.IsSmsVerificationEnabled || !profile.IsPhoneVerified)
                    throw new InvalidOperationException("SMS doğrulama aktif değil");
                isValid = true; // Dummy implementasyon
                break;

            case TwoFactorType.Email:
                if (!profile.IsEmailVerificationEnabled)
                    throw new InvalidOperationException("Email doğrulama aktif değil");
                isValid = true; // Dummy implementasyon
                break;
        }

        if (!isValid)
            throw new InvalidOperationException("Geçersiz doğrulama kodu");

        await _sessionService.Complete2FAAsync(session.Id);

        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            SessionId = session.Id,
            RequiresEmailVerification = false,
            Requires2FA = false
        };
    }

    public async Task<bool> SendTwoFactorCodeAsync(string sessionId, TwoFactorType type)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Geçersiz oturum");

        var user = await _context.Users.FindAsync(session.UserId);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı");

        var profile = await _profileService.GetProfileAsync(user.Id);
        if (profile == null)
            throw new InvalidOperationException("Kullanıcı profili bulunamadı");

        var code = GenerateVerificationCode();

        switch (type)
        {
            case TwoFactorType.Email:
                await _emailService.SendVerificationEmailAsync(user.Email, user.Username, code);
                break;

            // case TwoFactorType.Sms:
            //     if (profile.PhoneNumber == null)
            //         throw new InvalidOperationException("Telefon numarası bulunamadı");
            //     await _smsService.Send2FACodeAsync(profile.PhoneNumber, code);
            //     break;

            case TwoFactorType.GoogleAuth:
                throw new InvalidOperationException("Google Authenticator için kod gönderilmez");
        }

        return true;
    }

    public async Task<(string SecretKey, string QrCodeUri)> SetupGoogleAuthAsync(int userId, string email)
    {
        return await _profileService.EnableGoogleAuthAsync(userId, email);
    }

    public async Task<bool> SendEmailVerificationAsync(int userId, string email)
    {
        var code = GenerateVerificationCode();
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı");

        await _emailService.SendVerificationEmailAsync(email, user.Username, code);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(int userId, string token)
    {
        var session = await _sessionService.GetSessionAsync(token);
        if (session == null || session.UserId != userId)
            return false;

        return await _sessionService.VerifyEmailAsync(token, token);
    }

    public async Task<ApiKeyResponse> ValidateApiKeyAsync(string apiKey)
    {
        // API key formatı: {userId}:{tenantId}:{hash}
        var parts = apiKey.Split(':');
        if (parts.Length != 3)
            throw new InvalidOperationException("Geçersiz API key formatı");

        if (!int.TryParse(parts[0], out var userId) || !int.TryParse(parts[1], out var tenantId))
            throw new InvalidOperationException("Geçersiz API key formatı");

        var user = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Geçersiz API key");

        // API key hash kontrolü
        var expectedHash = GenerateApiKeyHash(userId, tenantId, user.PasswordHash);
        if (parts[2] != expectedHash)
            throw new InvalidOperationException("Geçersiz API key");

        var accessToken = await GenerateAccessTokenAsync(user);

        return new ApiKeyResponse
        {
            AccessToken = accessToken,
            ExpiresIn = 3600, // 1 saat
            TenantId = tenantId
        };
    }

    public async Task<string> GenerateApiKeyAsync(int userId, int tenantId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı");

        var hash = GenerateApiKeyHash(userId, tenantId, user.PasswordHash);
        return $"{userId}:{tenantId}:{hash}";
    }

    public Task<bool> RevokeApiKeyAsync(string apiKey)
    {
        // API key'i iptal etmek için kullanıcının şifresini değiştirmek yeterli
        // Şifre değiştiğinde eski hash geçersiz olacak
        return Task.FromResult(true);
    }

    public async Task<string> GenerateAccessTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("tenant_id", user.TenantId.ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            foreach (var permission in role.Permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiryMinutes")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<string> GenerateRefreshTokenAsync()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Task.FromResult(Convert.ToBase64String(randomNumber));
    }

    private static string GenerateVerificationCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }

    private static string GenerateApiKeyHash(int userId, int tenantId, string secret)
    {
        using var sha256 = SHA256.Create();
        var input = $"{userId}:{tenantId}:{secret}";
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<bool> ValidatePasswordAsync(string password)
    {
        return await Task.FromResult(true);
    }
} 