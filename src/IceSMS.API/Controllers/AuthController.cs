using IceSMS.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IceSMS.API.Models.Auth;
using IceSMS.API.Services;

namespace IceSMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu", userId = user.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var response = await _authService.LoginAsync(request, ipAddress, userAgent);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] string sessionId)
    {
        try
        {
            var result = await _authService.LogoutAsync(sessionId);
            return Ok(new { message = "Başarıyla çıkış yapıldı" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorRequest request)
    {
        try
        {
            var response = await _authService.VerifyTwoFactorAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("send-2fa-code")]
    [AllowAnonymous]
    public async Task<IActionResult> SendTwoFactorCode([FromBody] TwoFactorRequest request)
    {
        try
        {
            var result = await _authService.SendTwoFactorCodeAsync(request.SessionId, request.Type);
            return Ok(new { message = "Doğrulama kodu gönderildi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("setup-google-auth")]
    [Authorize]
    public async Task<IActionResult> SetupGoogleAuth()
    {
        try
        {
            var (secretKey, qrCodeUri) = await _authService.SetupGoogleAuthAsync(UserId, UserEmail);
            return Ok(new { secretKey, qrCodeUri });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("send-email-verification")]
    [Authorize]
    public async Task<IActionResult> SendEmailVerification([FromBody] string email)
    {
        try
        {
            var result = await _authService.SendEmailVerificationAsync(UserId, email);
            return Ok(new { message = "Doğrulama e-postası gönderildi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    [Authorize]
    public async Task<IActionResult> VerifyEmail([FromBody] string token)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(UserId, token);
            return Ok(new { message = "E-posta adresi doğrulandı" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("validate-api-key")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateApiKey([FromBody] ApiKeyRequest request)
    {
        try
        {
            var response = await _authService.ValidateApiKeyAsync(request.ApiKey);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("generate-api-key")]
    [Authorize]
    public async Task<IActionResult> GenerateApiKey()
    {
        try
        {
            var apiKey = await _authService.GenerateApiKeyAsync(UserId, TenantId);
            return Ok(new { apiKey });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("revoke-api-key")]
    [Authorize]
    public async Task<IActionResult> RevokeApiKey([FromBody] string apiKey)
    {
        try
        {
            var result = await _authService.RevokeApiKeyAsync(apiKey);
            return Ok(new { message = "API key iptal edildi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
} 