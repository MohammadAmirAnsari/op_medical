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
        Task<int> UpdateProfileAsync(SponsorProfile profile);
        Task<int> UpdatePasswordAsync(SponsorResetPassword request);
        Task<bool> IsSponsorPhoneNoAlreadyRegistered(string phoneNo);
    }

    public class SponsorService : ISponsorService
    {
        private readonly ITokenHelper _appTokenHelper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly AesEncryptionHelper _aesService;
        private readonly ISmsHelper _smsHelper;

        public SponsorService(IDbContextFactory<AppDbContext> contextFactory, ITokenHelper appTokenHelper, AesEncryptionHelper aesService, ISmsHelper smsHelper)
        {
            _contextFactory = contextFactory;
            _aesService = aesService;
            _smsHelper = smsHelper;
            _appTokenHelper = appTokenHelper;
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
                return await _db.SaveChangesAsync();
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
                        IsVerified = sponsor.IsVerified
                    };
                }
            }
            return null;
        }

        public async Task<int> UpdateProfileAsync(SponsorProfile profile)
        {
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            if(sponserId > 0)
            {
                using var _db = _contextFactory.CreateDbContext();
                Sponsor? sponsor = await _db.Sponsors.FirstOrDefaultAsync(x => x.Id == sponserId);
                if (sponsor != null)
                {
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
                    return await _db.SaveChangesAsync();
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
    }
}
