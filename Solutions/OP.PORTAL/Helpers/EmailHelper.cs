using MailKit.Net.Smtp;
using MimeKit;

namespace OP.PORTAL.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendMagicLinkAsync(string toEmail, string name, string magicLink)
        {
            try
            {
                var smtpServer = _configuration["SmtpSettings:Server"];
                var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var senderName = _configuration["SmtpSettings:SenderName"];
                var username = _configuration["SmtpSettings:Username"];
                var password = _configuration["SmtpSettings:Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail!));
                message.To.Add(new MailboxAddress(name, toEmail));
                message.Subject = "Verify Your Email Address - Oman Post";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h3>Marhaba {name},</h3>
                        <p>Thank you for registering on the Oman Post Visa Medical portal.</p>
                        <p>Click the link below to verify your email address. This link is valid for the next 24 hours:</p>
                        <br/><br/>
                        <p><a href='{magicLink}' target='_blank' style='background-color:#41b6e6; color:white; padding:10px 20px; text-decoration:none; border-radius:5px;'>Verify Email</a></p>
                        <br/>
                        <p>If the button doesn't work, copy-paste this URL into your browser:</p>
                        <code>{magicLink}</code>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer!, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    
                    await client.AuthenticateAsync(username!, password!);
                    
                    await client.SendAsync(message);
                    
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Error: {ex.Message}");
                return false;
            }
        }
    }
}
