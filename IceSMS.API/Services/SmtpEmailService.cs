using System.Net;
using System.Net.Mail;

namespace IceSMS.API.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SmtpEmailService(IConfiguration configuration)
    {
        _smtpClient = new SmtpClient
        {
            Host = configuration["Smtp:Host"] ?? "smtp.gmail.com",
            Port = configuration.GetValue<int>("Smtp:Port"),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                configuration["Smtp:Username"],
                configuration["Smtp:Password"]
            )
        };

        _fromEmail = configuration["Smtp:FromEmail"] ?? configuration["Smtp:Username"] ?? "";
        _fromName = configuration["Smtp:FromName"] ?? "IceSMS";
    }

    public async Task SendVerificationEmailAsync(string email, string name, string verificationToken)
    {
        var subject = "Email Adresinizi Doğrulayın";
        var body = $@"
            <h2>Merhaba {name},</h2>
            <p>Email adresinizi doğrulamak için aşağıdaki kodu kullanın:</p>
            <h3>{verificationToken}</h3>
            <p>Bu kod 30 dakika süreyle geçerlidir.</p>
            <p>Eğer bu işlemi siz yapmadıysanız, lütfen bu emaili dikkate almayın.</p>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string name, string resetToken)
    {
        var subject = "Şifre Sıfırlama";
        var body = $@"
            <h2>Merhaba {name},</h2>
            <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>
            <h3>{resetToken}</h3>
            <p>Bu kod 30 dakika süreyle geçerlidir.</p>
            <p>Eğer bu işlemi siz yapmadıysanız, lütfen bu emaili dikkate almayın.</p>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendLoginNotificationAsync(string email, string name, string ipAddress, string userAgent)
    {
        var subject = "Yeni Giriş Bildirimi";
        var body = $@"
            <h2>Merhaba {name},</h2>
            <p>Hesabınıza yeni bir giriş yapıldı:</p>
            <ul>
                <li>IP Adresi: {ipAddress}</li>
                <li>Tarayıcı: {userAgent}</li>
                <li>Tarih: {DateTime.UtcNow:g}</li>
            </ul>
            <p>Eğer bu işlemi siz yapmadıysanız, lütfen hemen şifrenizi değiştirin.</p>";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await _smtpClient.SendMailAsync(mailMessage);
    }
} 