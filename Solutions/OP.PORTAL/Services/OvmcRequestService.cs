using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OP.PORTAL.Data;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;
using System;
using System.Collections;
using System.Diagnostics.Metrics;

namespace OP.PORTAL.Services
{
    public interface IOvmcRequestService
    {
        Task<List<OvmcRequest>> GetAllAsync();
        Task<OvmcRequest?> GetAsync(string encidt);
        Task<dynamic?> GetByUrnAsync(string urnNumber);
        Task<int> AddAsync(OvmcRequest request);
        Task<int> UpdateAsync(OvmcRequest request);
        Task<int> UpdateByUrnAsync(OvmcRequestStatusDto request);
        Task<int> UpdateByUrnDataAsync(string urnNumber, OvmcRequestDto request);
        Task<int> DeleteAsync(int id);
        Task<int> PaymentSuccess(int id, string urnNumber);
    }

    public class OvmcRequestService : IOvmcRequestService
    {
        private readonly ITokenHelper _appTokenHelper;
        private readonly AesEncryptionHelper _aesService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;


        public OvmcRequestService(IDbContextFactory<AppDbContext> contextFactory, ITokenHelper appTokenHelper, AesEncryptionHelper aesService)
        {
            _contextFactory = contextFactory;
            _appTokenHelper = appTokenHelper;
            _aesService = aesService;
        }

        public async Task<List<OvmcRequest>> GetAllAsync()
        {
            using var _db = _contextFactory.CreateDbContext();
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            return await _db.OvmcRequests.AsNoTracking().Where(r => r.SponserId == sponserId).OrderByDescending(x => x.Id).ToListAsync();
        }

        public async Task<OvmcRequest?> GetAsync(string encidt)
        {
            int id = Convert.ToInt32(_aesService.DecryptUrl(encidt));
            using var _db = _contextFactory.CreateDbContext();
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            return await _db.OvmcRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id && r.SponserId == sponserId);
        }

        public async Task<int> AddAsync(OvmcRequest product)
        {
            if(product.Id > 0)
            {
                using var _db = _contextFactory.CreateDbContext();

                var existing = await _db.OvmcRequests.FindAsync(product.Id);
                if (existing == null) return 0;

                product.ModifiedDate = DateTime.Now;
                product.LastModifiedBy = await _appTokenHelper.GetUsername();
                _db.Entry(existing).CurrentValues.SetValues(product);
                //_db.OvmcRequests.Update(product);
                return await _db.SaveChangesAsync();
            }
            else
            {
                using var _db = _contextFactory.CreateDbContext();
                product.SponserId = await _appTokenHelper.GetSponsorId() ?? 0;
                product.LastModifiedBy = await _appTokenHelper.GetUsername();
                _db.OvmcRequests.Add(product);
                return await _db.SaveChangesAsync();
            }                
        }

        public async Task<int> UpdateAsync(OvmcRequest product)
        {
            using var _db = _contextFactory.CreateDbContext();
            product.ModifiedDate = DateTime.Now;
            product.LastModifiedBy = await _appTokenHelper.GetUsername();
            _db.OvmcRequests.Update(product);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var _db = _contextFactory.CreateDbContext();
            var product = await _db.OvmcRequests.FindAsync(id);
            if (product == null) return 0;

            _db.OvmcRequests.Remove(product);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> PaymentSuccess(int id, string urnNumber)
        {
            using var _db = _contextFactory.CreateDbContext();
            var product = await _db.OvmcRequests.FindAsync(id);
            if (product == null) return 0;

            product.OvmcUrnNumber = urnNumber;
            product.RequestStatus = OvmcRequestStatus.SUBMITTED;
            product.PaymentStatus = OvmcPaymentStatus.SUCCESS;
            product.ModifiedDate = DateTime.Now;
            product.LastModifiedBy = await _appTokenHelper.GetUsername();

            _db.OvmcRequests.Update(product);
            return await _db.SaveChangesAsync();
        }

        public async Task<dynamic?> GetByUrnAsync(string urnNumber)
        {
            if(string.IsNullOrEmpty(urnNumber))
                throw new ArgumentNullException(nameof(urnNumber));

            using var _db = _contextFactory.CreateDbContext();
            return await _db.OvmcRequests
                .AsNoTracking()
                .Join(
                    _db.Sponsors.AsNoTracking(),
                    r => r.SponserId,
                    c => c.Id,
                    (r, c) => new OvmcRequestDto
                    {
                        SponserName = c.Name,
                        SponsorPhoneNo = c.PhoneNo,
                        SponsorCity = c.City,
                        SponsorType = c.SponsorType,
                        SponsorEmail = c.Email,
                        VisaType = r.VisaType,
                        OvmcUrnNumber = r.OvmcUrnNumber,
                        MolWorkPermitNo = r.MolWorkPermitNo,
                        Country = r.Country,
                        City = r.City,
                        PassportNo = r.PassportNo,
                        PassportIssuePlace = r.PassportIssuePlace,
                        PassportIssueDate = r.PassportIssueDate,
                        PassportExpiryDate = r.PassportExpiryDate,
                        GivenName = r.GivenName,
                        Surname = r.Surname ?? string.Empty,
                        DateOfBirth = r.DateOfBirth,
                        Gender = r.Gender ?? string.Empty,
                        MaritalStatus = r.MaritalStatus ?? string.Empty,
                        PhoneNo = r.PhoneNo,
                        Email = r.Email,
                        RequestStatus = r.RequestStatus,
                        PaymentStatus = r.PaymentStatus,
                        CreatedDate = r.CreatedDate
                    }
                )
                .FirstOrDefaultAsync(r => !string.IsNullOrEmpty(r.OvmcUrnNumber) && (r.OvmcUrnNumber.Equals(urnNumber) || r.PassportNo.Equals(urnNumber)));
        }

        public async Task<int> UpdateByUrnDataAsync(string urnNumber,OvmcRequestDto request)
        {
            if (!string.IsNullOrEmpty(urnNumber))
            {
                using var _db = _contextFactory.CreateDbContext();
                var product = await _db.OvmcRequests.FirstOrDefaultAsync(r => r.OvmcUrnNumber.Equals(urnNumber)
                && !(new[]
                {
                    OvmcRequestStatus.CANCELLED,
                    OvmcRequestStatus.COMPLETED
                }.Contains(r.RequestStatus)));

                if (product == null) return 0;

                product.ModifiedDate = DateTime.Now;
                product.VisaType = request.VisaType;
                product.MolWorkPermitNo = request.MolWorkPermitNo;
                product.City = request.City;
                product.PassportIssuePlace = request.PassportIssuePlace;
                product.PassportIssueDate = request.PassportIssueDate;
                product.PassportExpiryDate = request.PassportExpiryDate;
                product.GivenName = request.GivenName;
                product.Surname = request.Surname;
                product.DateOfBirth = request.DateOfBirth;
                product.Gender = request.Gender;
                product.MaritalStatus = request.MaritalStatus;
                product.PhoneNo = request.PhoneNo;
                product.Email = request.Email;
                
                
                _db.OvmcRequests.Update(product);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }
    

        public async Task<int> UpdateByUrnAsync(OvmcRequestStatusDto request)
        {
            if (new[]
            {
                OvmcRequestStatus.CANCELLED,
                OvmcRequestStatus.INPROCESS,
                OvmcRequestStatus.NOTAPPAIRED,
                OvmcRequestStatus.COMPLETED
            }.Contains(request.Status))
            {
                using var _db = _contextFactory.CreateDbContext();
                var product = await _db.OvmcRequests.FirstOrDefaultAsync(r => r.OvmcUrnNumber.Equals(request.OvmcUrnNumber)
                && !(new[]
                {
                    OvmcRequestStatus.CANCELLED,
                    OvmcRequestStatus.COMPLETED
                }.Contains(r.RequestStatus)));

                if (product == null) return 0;

                product.RequestStatus = request.Status;
                product.ModifiedDate = DateTime.Now;
                product.LastModifiedBy = request.LastModifiedBy;
                _db.OvmcRequests.Update(product);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }
    }
}
