using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using OP.PORTAL.Data;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OP.PORTAL.Services
{
    public class AuthService
    {        
        private readonly NavigationManager _navigationManager;
        private readonly ITokenHelper _appTokenHelper;
        private readonly ISmsHelper _smsHelper;
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IStringLocalizer<AuthService> _localizer;
        private readonly AesEncryptionHelper _aesEncryptionHelper;

        public AuthService(IDbContextFactory<AppDbContext> contextFactory, ISmsHelper smsHelper, IStringLocalizer<AuthService> localizer,
            ITokenHelper appTokenHelper, NavigationManager navigationManager, IConfiguration configuration, AesEncryptionHelper aesEncryptionHelper)
        {
            _navigationManager = navigationManager;
            _appTokenHelper = appTokenHelper;
            _configuration = configuration;
            _contextFactory = contextFactory;
            _smsHelper = smsHelper;
            _localizer = localizer;
            _aesEncryptionHelper = aesEncryptionHelper;
        }

        public async Task<SmsResponse> SendOtpAsync(string phoneNo, string password, bool fromLogin = true)
        {
            using var _db = _contextFactory.CreateDbContext();
            Sponsor? sponsor = _db.Sponsors.Where(s => s.PhoneNo == phoneNo && s.IsVerified == true).FirstOrDefault();

            if (sponsor != null)
            {
                if (fromLogin)
                {
                    var hasher = new PasswordHasher<string>();
                    var result = hasher.VerifyHashedPassword(sponsor.PhoneNo, sponsor.PasswordHash, password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        return await _smsHelper.SendOtpAsync(phoneNo);
                    }
                }
                else
                {
                    string nationalId = _aesEncryptionHelper.Encrypt(password);
                    if (sponsor.NationalId.Equals(nationalId))
                    {
                        return await _smsHelper.SendOtpAsync(phoneNo, "RESETPASSWORD");
                    }
                }                    
            }
            return new SmsResponse { Status = false, Message = fromLogin ? _localizer["_InvalidUsernameOrPassword"] : _localizer["_InvalidUsernameOrNationalId"] };
        }

        public async Task<string?> LoginAsync(string phoneNo, string password, string VerifyOtp)
        {
            try
            {
                //verify otp code
                if (await _smsHelper.VerifyOtpAsync(phoneNo, VerifyOtp))
                {
                    using var _db = _contextFactory.CreateDbContext();
                    Sponsor? sponsor = _db.Sponsors.Where(s => s.PhoneNo == phoneNo && s.IsVerified == true).FirstOrDefault();

                    if(sponsor != null)
                    {
                        var hasher = new PasswordHasher<string>();
                        var result = hasher.VerifyHashedPassword(sponsor.PhoneNo, sponsor.PasswordHash, password);

                        if (result == PasswordVerificationResult.Success)
                        {
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);

                            var tokenDescriptor = new SecurityTokenDescriptor
                            {
                                Subject = new ClaimsIdentity(new[]
                                {
                                    new System.Security.Claims.Claim("Name", _aesEncryptionHelper.Encrypt(sponsor.Name.ToString())),
                                    new System.Security.Claims.Claim("Id", _aesEncryptionHelper.Encrypt(sponsor.Id.ToString()))
                                }),
                                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                                Issuer = _configuration["Jwt:Issuer"],
                                Audience = _configuration["Jwt:Audience"],
                                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                            };

                            var token = tokenHandler.CreateToken(tokenDescriptor);
                            var tokenString = tokenHandler.WriteToken(token);

                            if (!string.IsNullOrEmpty(tokenString))
                            {
                                sponsor.LastModifiedBy = sponsor.Name;
                                sponsor.LastLoginDate = DateTime.Now;
                                _db.Sponsors.Update(sponsor);
                                await _db.SaveChangesAsync();
                            }

                            await _appTokenHelper.SetTokenAsync(tokenString);
                            return tokenString ?? null;
                        }                                                
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<SmsResponse> ResetPassword(SponsorForgotPassword model)
        {
            try
            {
                if (await _smsHelper.VerifyOtpAsync(model.PhoneNo, model.VerifyOtp, "RESETPASSWORD"))
                {
                    string nationalId = _aesEncryptionHelper.Encrypt(model.NationalId);
                    using var _db = _contextFactory.CreateDbContext();
                    Sponsor? sponsor = _db.Sponsors.Where(s => s.PhoneNo == model.PhoneNo && s.NationalId.Equals(nationalId) && s.IsVerified == true).FirstOrDefault();
                    if (sponsor != null)
                    {
                        var hasher = new PasswordHasher<string>();
                        sponsor.PasswordHash = hasher.HashPassword(sponsor.PhoneNo, model.Password);
                        sponsor.LastModifiedBy = sponsor.Name;
                        _db.Sponsors.Update(sponsor);
                        await _db.SaveChangesAsync();
                        return new SmsResponse { Status = true, Message = _localizer["_PasswordResetSuccess"] };
                    }
                    return new SmsResponse { Status = false, Message = _localizer["_UserNotFound"] };
                }
                return new SmsResponse { Status = false, Message = _localizer["_InvalidOtp"] };
            }
            catch (Exception ex)
            {
                return new SmsResponse { Status = false, Message = _localizer["_PasswordResetFailed"] };
            }
        }

        public async Task LogoutAsync()
        {
            await _appTokenHelper.RemoveTokenAsync();
        }
    }
}
