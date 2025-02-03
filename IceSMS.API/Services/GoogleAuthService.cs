using OtpNet;
using System.Text;

namespace IceSMS.API.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private const string Issuer = "IceSMS";

    public (string SecretKey, string QrCodeUri) GenerateSetupInfo(string email)
    {
        var secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var qrCodeUri = $"otpauth://totp/{Issuer}:{email}?secret={secretKey}&issuer={Issuer}";
        return (secretKey, qrCodeUri);
    }

    public bool ValidateCode(string secretKey, string code)
    {
        var key = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(key);
        return totp.VerifyTotp(code, out _);
    }
} 