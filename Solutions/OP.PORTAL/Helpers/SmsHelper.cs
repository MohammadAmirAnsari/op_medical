using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Localization;
using OP.PORTAL.Controllers;
using OP.PORTAL.Data;
using OP.PORTAL.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OP.PORTAL.Helpers
{
    public interface ISmsHelper
    {
        Task<SmsResponse> SendOtpAsync(string phoneNo, string otpFor = "LOGIN");
        Task<bool> VerifyOtpAsync(string phoneNo, string Otp, string otpVerifyFor = "LOGIN");
        Task<SmsResponse> SendSmsAsync(SmsRequest smsRequest);
    }

    public class SmsHelper : ISmsHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IStringLocalizer<SmsHelper> _localizer;

        public SmsHelper(IDbContextFactory<AppDbContext> contextFactory, IStringLocalizer<SmsHelper> localizer,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._contextFactory = contextFactory;
            this._configuration = configuration;
            this._httpContextAccessor = httpContextAccessor;
            this._localizer = localizer;
        }

        private async Task<SmsResponse> TamimahApi(SmsRequest smsRequest)
        {
            string soapString = @"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                      <soap12:Body>
                        <SendSMS xmlns=""https://www.tamimahsms.com/"">
                          <UserName>" + _configuration["Sms:TAMIMAH:Username"] + "</UserName>" +
                          "<Password>" + _configuration["Sms:TAMIMAH:Password"] + "</Password> " +
                          "<Message>" + smsRequest.Message + "</Message> " +
                          "<Priority>1</Priority> " +
                          "<Schdate></Schdate> " +
                          "<Sender>" + _configuration["Sms:TAMIMAH:SenderID"] + "</Sender> " +
                          "<AppID>" + _configuration["Sms:TAMIMAH:AppID"] + "</AppID> " +
                          "<SourceRef>" + _configuration["Sms:TAMIMAH:SourceRef"] + "</SourceRef> " +
                          "<MSISDNs>" + smsRequest.PhoneNo + "</MSISDNs> " +
                        "</SendSMS> " +
                      "</soap12:Body> " +
                    "</soap12:Envelope>";

            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(soapString, Encoding.UTF8, "application/soap+xml");
                HttpResponseMessage response = await httpClient.PostAsync(_configuration["Sms:TAMIMAH:URL"], httpContent);
                return new SmsResponse
                {
                    Status = response.IsSuccessStatusCode,
                    Message = await response.Content.ReadAsStringAsync()
                };
            }
        }

        private async Task<SmsResponse> IMessageApi(SmsRequest smsRequest)
        {
            string jsonString = JsonSerializer.Serialize(new
            {
                BankCode = _configuration["Sms:IMESSAGE:BankCode"],
                BankPWD = _configuration["Sms:IMESSAGE:BankPWD"],
                SenderID = _configuration["Sms:IMESSAGE:SenderID"],
                MsgText = smsRequest.Message,
                MobileNo = smsRequest.PhoneNo
            });

            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(_configuration["Sms:IMESSAGE:URL"], httpContent);
                return new SmsResponse
                {
                    Status = response.IsSuccessStatusCode,
                    Message = await response.Content.ReadAsStringAsync()
                };
            }
        }

        private async Task<SmsResponse> CreateSoapEnvelope(SmsRequest smsRequest)
        {
            string? smsProvider = Convert.ToString(this._configuration.GetSection("Sms:Provider").Value);

            return smsProvider switch
            {
                "TAMIMAH" => await this.TamimahApi(smsRequest),
                "IMESSAGE" => await this.IMessageApi(smsRequest),
                _ => new SmsResponse { Status =  false, Message = _localizer["_SmsProviderNotConfigured"] },
            };
        }

        public async Task<SmsResponse> SendSmsAsync(SmsRequest smsRequest)
        {
            try
            {
                return await CreateSoapEnvelope(smsRequest);
            }
            catch (Exception ex)
            {
                return new SmsResponse { Status = false, Message = ex.Message };
            }
        }

        public async Task<SmsResponse> SendOtpAsync(string phoneNo, string otpFor = "LOGIN")
        {
            try
            {                
                bool smsEnabled = Convert.ToBoolean(this._configuration.GetSection("Sms:Enabled").Value);
                string? smsProvider = Convert.ToString(this._configuration.GetSection("Sms:Provider").Value);
                string? smsDefaultOtp = Convert.ToString(this._configuration.GetSection("Sms:DefaultOtp").Value);

                if (!string.IsNullOrEmpty(phoneNo) && !string.IsNullOrEmpty(smsProvider))
                {
                    using var _db = _contextFactory.CreateDbContext();
                    SmsRequest smsRequest = new()
                    {
                        PhoneNo = phoneNo,
                        CreatedDate = DateTime.Now,
                        ExpiryTime = DateTime.Now.AddMinutes(15),
                        IsUsed = false,
                        OtpFor = otpFor
                    };
                    var hasher = new PasswordHasher<string>();
                    string? otpHash = !smsEnabled ? smsDefaultOtp : RandomNumberGenerator.GetInt32(100000, 999999).ToString();
                    smsRequest.OtpHash = hasher.HashPassword(smsRequest.PhoneNo, otpHash ?? string.Empty);
                    smsRequest.Message = _localizer["_LoginOtpSmsTemplate", otpHash ?? string.Empty];
                    _db.SmsRequests.Add(smsRequest);
                    if(await _db.SaveChangesAsync() > 0)
                    {
                        return !smsEnabled ? new SmsResponse { Status = true, Message = _localizer["_SmsDefaultOptUse", smsDefaultOtp ?? string.Empty] } : await CreateSoapEnvelope(smsRequest);
                    }                        
                }
                return new SmsResponse { Status = false, Message = _localizer["_SmsProviderNotConfigured"] };
            }
            catch (Exception ex)
            {
                return new SmsResponse { Status = false, Message = ex.Message };
            }
        }

        public async Task<bool> VerifyOtpAsync(string phoneNo, string Otp, string otpVerifyFor = "LOGIN")
        {
            using var _db = _contextFactory.CreateDbContext();
            SmsRequest? smslog = await _db.SmsRequests.AsNoTracking()
                .Where(s => s.PhoneNo.Equals(phoneNo)
                && s.OtpFor.Equals(otpVerifyFor) && !s.IsUsed
                && s.ExpiryTime >= DateTime.Now).OrderByDescending(s => s.Id).FirstOrDefaultAsync();

            if (smslog == null) return false;
            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(smslog.PhoneNo, smslog.OtpHash, Otp);

            smslog.IsUsed = true;
            smslog.UsedTime = DateTime.Now;
            _db.SmsRequests.Update(smslog);
            await _db.SaveChangesAsync();
            return result == PasswordVerificationResult.Success;
        }
    }
}
