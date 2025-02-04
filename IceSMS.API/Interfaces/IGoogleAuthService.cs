namespace IceSMS.API.Interfaces;

public interface IGoogleAuthService
{
    (string SecretKey, string QrCodeUri) GenerateSetupInfo(string email);
    bool ValidateCode(string secretKey, string code);
} 