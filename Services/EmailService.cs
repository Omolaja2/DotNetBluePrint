using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetBlueprint.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendWelcomeEmailAsync(string email)
        {
            var fromEmail = _config["EmailSettings:FromEmail"];
            var password = _config["EmailSettings:Password"];
            var smtpHost = _config["EmailSettings:SmtpHost"];
            var smtpPortStr = _config["EmailSettings:SmtpPort"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPortStr))
            {
                _logger.LogError("Email configurations are missing. Cannot send real email.");
                return;
            }

            int smtpPort = int.Parse(smtpPortStr);

            var htmlTemplate = $@"
<html>
<head>
    <meta charset='utf-8'>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;700;900&display=swap');
        body {{ font-family: 'Inter', sans-serif; background-color: #faf9f6; color: #1a1412; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 40px auto; background: #ffffff; border-radius: 32px; overflow: hidden; box-shadow: 0 20px 40px rgba(92, 59, 46, 0.1); border: 1px solid #ede9e6; }}
        .header {{ background: #5c3b2e; padding: 60px 20px; text-align: center; }}
        .logo-circle {{ width: 80px; height: 80px; background: #ffffff; border-radius: 50%; display: inline-flex; align-items: center; justify-content: center; margin-bottom: 20px; box-shadow: 0 8px 20px rgba(0,0,0,0.2); }}
        .content {{ padding: 60px 40px; text-align: center; }}
        .title {{ color: #5c3b2e; font-size: 32px; font-weight: 900; margin-bottom: 20px; letter-spacing: -1px; }}
        .message {{ line-height: 1.8; color: #7a6e67; font-size: 17px; margin-bottom: 40px; }}
        .btn {{ background: #5c3b2e; color: #ffffff !important; padding: 18px 40px; border-radius: 50px; text-decoration: none; font-weight: 700; display: inline-block; font-size: 16px; box-shadow: 0 10px 20px rgba(92, 59, 46, 0.3); }}
        .footer {{ padding: 30px; background: #faf9f6; text-align: center; font-size: 13px; color: #a1958e; border-top: 1px solid #ede9e6; }}
        .badge {{ display: inline-block; padding: 6px 12px; background: rgba(215, 191, 174, 0.2); color: #5c3b2e; border-radius: 50px; font-weight: 700; font-size: 12px; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo-circle'>
                <span style='font-size: 40px;'>🏗️</span>
            </div>
            <h2 style='color: #ffffff; margin: 0; letter-spacing: 2px; font-weight: 900;'>BLUEPRINT STUDIO</h2>
        </div>
        <div class='content'>
            <span class='badge'>FORGE ACCESS GRANTED</span>
            <h1 class='title'>Welcome, Architect.</h1>
            <p class='message'>Your workshop is ready. You now have full access to the most sophisticated .NET scaffolding engine in the industry. Start forging project blueprints that follow the highest standards of architectural excellence.</p>
            <a href='https://dotnetblueprint.onrender.com/Generator/Create' class='btn'>ENTER THE STUDIO</a>
        </div>
        <div class='footer'>
            &copy; 2026 Blueprint Studio. All rights reserved.<br>
            Sent with precision to {email}<br>
            <span style='margin-top: 10px; display: block;'>Sophisticated .NET Scaffolding Engine.</span>
        </div>
    </div>
</body>
</html>";

            try
            {
                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, password);
                    smtp.EnableSsl = true;

                    var message = new MailMessage
                    {
                        From = new MailAddress(fromEmail!),
                        Subject = "Welcome to the Forge | Blueprint Studio",
                        Body = htmlTemplate,
                        IsBodyHtml = true
                    };

                    message.To.Add(email);

                    await smtp.SendMailAsync(message);
                    _logger.LogInformation("✅ REAL EMAIL SENT TO: {Email}", email);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "❌ FAILED TO SEND REAL EMAIL TO: {Email}", email);
            }
        }
    }
}
