using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OP.PORTAL.Helpers
{
    public class EmailHelper
    {
        public static async Task<bool> SendMagicLinkAsync(string toEmail, string name, string magicLink)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("no-reply@omanpost.om", "OmanPost - no-reply");
                mail.To.Add(toEmail);
                mail.Subject = "Verify Your Email Address - Oman Post";
                
                mail.Body = $@"
                    <h3>Marhaba {name},</h3>
                    <p>Thank you for registering on the Oman Post Visa Medical portal.</p>
                    <p>Click the link below to verify your email address. This link is valid for the next 24 hours:</p>
                    <br/><br/>
                    <p><a href='{magicLink}' target='_blank' style='background-color:#41b6e6; color:white; padding:10px 20px; text-decoration:none; border-radius:5px;'>Verify Email Now</a></p>
                    <br/>
                    <p>If the button doesn't work, copy-paste this URL into your browser:</p>
                    <code>{magicLink}</a></code>";
                
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("no-reply@omanpost.om", "ASY@check$1234!");
                    smtp.EnableSsl = true; // TLS/SSL enabled
                    smtp.Timeout = 90000; // 90 seconds timeout
                    
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    
                    await smtp.SendMailAsync(mail);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
