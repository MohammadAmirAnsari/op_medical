using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OP.PORTAL.Components.Pages;
using OP.PORTAL.Data;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;

namespace OP.PORTAL.Services
{
    public interface ISponsorService
    {
        Task<List<Sponsor>> GetAllAsync();
        Task<SponsorProfile?> GetProfile();
        Task<int> AddAsync(SponsorRegister request);
        Task<int> UpdateProfileAsync(SponsorProfile profile, bool isEmailChanged);
        Task<int> UpdatePasswordAsync(SponsorResetPassword request);
        Task<bool> IsSponsorPhoneNoAlreadyRegistered(string phoneNo);
        Task<string> GenerateAndSaveEmailTokenAsync(int sponsorId);
        Task<bool> VerifyEmailTokenAsync(string token);
    }

    public class SponsorService : ISponsorService
    {
        private readonly ITokenHelper _appTokenHelper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly AesEncryptionHelper _aesService;
        private readonly ISmsHelper _smsHelper;

        private readonly IServiceProvider _serviceProvider;


        public SponsorService(IDbContextFactory<AppDbContext> contextFactory, ITokenHelper appTokenHelper, AesEncryptionHelper aesService, ISmsHelper smsHelper,IServiceProvider serviceProvider)
        {
            _contextFactory = contextFactory;
            _aesService = aesService;
            _smsHelper = smsHelper;
            _appTokenHelper = appTokenHelper;
            _serviceProvider = serviceProvider;
        }

        public async Task<List<Sponsor>> GetAllAsync()
        {
            using var _db = _contextFactory.CreateDbContext();
            return await _db.Sponsors.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync();
        }      
        
        public async Task<bool> IsSponsorPhoneNoAlreadyRegistered(string phoneNo)
        {
            using var _db = _contextFactory.CreateDbContext();
            return await _db.Sponsors.AsNoTracking().AnyAsync(x => x.PhoneNo.Equals(phoneNo));
        }

        public async Task<int> AddAsync(SponsorRegister sponsor)
        {
            if (await _smsHelper.VerifyOtpAsync(sponsor.PhoneNo, sponsor.VerifyOtp, "REGISTER"))
            {
                using var _db = _contextFactory.CreateDbContext();
                var hasher = new PasswordHasher<string>();
                sponsor.NationalId = _aesService.Encrypt(sponsor.NationalId);
                sponsor.PasswordHash = hasher.HashPassword(sponsor.PhoneNo, sponsor.PasswordHash);
                _db.Sponsors.Add(sponsor);
                return await _db.SaveChangesAsync() > 0 ? sponsor.Id : 0;
            }
            return 0;
        }

        public async Task<SponsorProfile?> GetProfile()
        {
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            if (sponserId > 0)
            {
                using var _db = _contextFactory.CreateDbContext();
                Sponsor? sponsor = await _db.Sponsors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sponserId);
                if (sponsor != null)
                {
                    return new SponsorProfile
                    {
                        Id = sponsor.Id,
                        SponsorType = sponsor.SponsorType,
                        Name = sponsor.Name,
                        NationalId = _aesService.Decrypt(sponsor.NationalId),
                        PhoneNo = sponsor.PhoneNo,
                        Email = sponsor.Email,
                        City = sponsor.City,
                        IsVerified = sponsor.IsVerified,
                        IsEmailVerified = sponsor.IsEmailVerified
                    };
                }
            }
            return null;
        }

        public async Task<int> UpdateProfileAsync(SponsorProfile profile, bool isEmailChanged)
        {
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            if(sponserId > 0)
            {
                using var _db = _contextFactory.CreateDbContext();
                Sponsor? sponsor = await _db.Sponsors.FirstOrDefaultAsync(x => x.Id == sponserId);
                if (sponsor != null)
                {
                    if(isEmailChanged)
                    {
                        sponsor.IsEmailVerified = false;
                    }

                    sponsor.IsVerified = true;
                    sponsor.SponsorType = profile.SponsorType;
                    sponsor.NationalId = _aesService.Encrypt(profile.NationalId);
                    sponsor.Name = profile.Name;
                    sponsor.Email = profile.Email;
                    sponsor.City = profile.City;
                    sponsor.PhoneNo = profile.PhoneNo;
                    sponsor.ModifiedDate = DateTime.Now;
                    sponsor.LastModifiedBy = await _appTokenHelper.GetUsername();
                    _db.Sponsors.Update(sponsor);
                    return await _db.SaveChangesAsync() > 0 ? sponsor.Id : 0;
                }
            }
            return 0;
        }

        public async Task<int> UpdatePasswordAsync(SponsorResetPassword request)
        {
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            if (sponserId > 0)
            {
                using var _db = _contextFactory.CreateDbContext();
                Sponsor? sponsor = await _db.Sponsors.FirstOrDefaultAsync(x => x.Id == sponserId);
                if (sponsor != null)
                {
                    var hasher = new PasswordHasher<string>();
                    sponsor.PasswordHash = hasher.HashPassword(sponsor.PhoneNo, request.Password);
                    sponsor.ModifiedDate = DateTime.Now;
                    sponsor.LastModifiedBy = await _appTokenHelper.GetUsername();
                    _db.Sponsors.Update(sponsor);
                    return await _db.SaveChangesAsync();
                }
            }
            return 0;
        }

        public async Task<string> GenerateAndSaveEmailTokenAsync(int sponsorId)
        {
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sponsor = await dbContext.Sponsors.FindAsync(sponsorId);
            if (sponsor != null)
            {
                sponsor.EmailVerificationToken = token;
                sponsor.EmailTokenExpiry = DateTime.UtcNow.AddHours(24);
                await dbContext.SaveChangesAsync();
            }
            return token;
        }

        public async Task<bool> VerifyEmailTokenAsync(string token)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sponsor = await dbContext.Sponsors.FirstOrDefaultAsync(s => s.EmailVerificationToken == token);
            if (sponsor == null || sponsor.EmailTokenExpiry < DateTime.UtcNow)
            {
                return false;
            }

            sponsor.IsEmailVerified = true;
            sponsor.EmailVerificationToken = null;
            sponsor.EmailTokenExpiry = null;
            await dbContext.SaveChangesAsync();
            return true;
        }

    }
}
