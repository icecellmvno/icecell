namespace IceSMS.API.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string name, string verificationToken);
    Task SendPasswordResetEmailAsync(string email, string name, string resetToken);
    Task SendLoginNotificationAsync(string email, string name, string ipAddress, string userAgent);
} 