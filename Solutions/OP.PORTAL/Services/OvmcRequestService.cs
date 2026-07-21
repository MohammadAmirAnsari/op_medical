using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OP.PORTAL.Data;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;
using System;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;

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
        Task<(bool IsValid, string Key, string Message)> PatchByUrnDataAsync_New(JsonObject request);
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


        public async Task<(bool IsValid, string Key, string Message)> PatchByUrnDataAsync_New(JsonObject payload)
        {
            using var _db = _contextFactory.CreateDbContext();

            var urn = payload["OvmcUrnNumber"]?.ToString();
            if (string.IsNullOrEmpty(urn)) return (false, "OvmcUrnNumber", "OvmcUrnNumber is required.");

            // we can edit the record only if the status are not COMPLETED
            var existingRecord = await _db.OvmcRequests.FirstOrDefaultAsync(r =>
                r.OvmcUrnNumber.Equals(urn)
            );

            if (existingRecord == null) return (false, "OvmcUrnNumber", "Record Not Found.");

            if (existingRecord.RequestStatus == OvmcRequestStatus.COMPLETED)
                return (false, "OvmcUrnNumber", "Update not allowed. The record is already in '" + existingRecord.RequestStatus + "' status.");

            

            // we cannot update passport number if the status is SUBMITTED and payment is SUCCESS
            if (existingRecord.RequestStatus == OvmcRequestStatus.SUBMITTED && existingRecord.PaymentStatus == OvmcPaymentStatus.SUCCESS)
            {
                if (payload.ContainsKey("PassportNo"))
                    return (false, "PassportNo", "Passport number cannot be updated after successful submission.");
            }

            existingRecord.ModifiedDate = DateTime.Now;
            var context = new ValidationContext(existingRecord);

            // VisaType
            if (payload.ContainsKey("VisaType"))
            {
                var val = payload["VisaType"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "VisaType", "VisaType cannot be empty.");
                existingRecord.VisaType = val;
            }

            // MolWorkPermitNo
            if (payload.ContainsKey("MolWorkPermitNo"))
            {
                var val = payload["MolWorkPermitNo"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "MolWorkPermitNo", "MolWorkPermitNo cannot be empty.");

                var attr = new OnlyNumberAttribute(8, 8);
                var res = attr.GetValidationResult(val, context);
                if (res != ValidationResult.Success) return (false, "MolWorkPermitNo", res?.ErrorMessage ?? "Invalid Work Permit Number.");
                existingRecord.MolWorkPermitNo = val;
            }

            // City
            if (payload.ContainsKey("City"))
            {
                var val = payload["City"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "City", "City cannot be empty.");
                existingRecord.City = val;
            }

            // PassportIssuePlace
            if (payload.ContainsKey("PassportIssuePlace"))
            {
                var val = payload["PassportIssuePlace"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "PassportIssuePlace", "PassportIssuePlace cannot be empty.");

                var attr = new NoArabicAttribute();
                var res = attr.GetValidationResult(val, context);
                if (res != ValidationResult.Success) return (false, "PassportIssuePlace", res?.ErrorMessage ?? "Arabic characters not allowed.");
                existingRecord.PassportIssuePlace = val;
            }

            // PassportIssueDate
            if (payload.ContainsKey("PassportIssueDate"))
            {
                var val = payload["PassportIssueDate"]?.ToString();
                if (!DateTime.TryParse(val, out DateTime date)) return (false, "PassportIssueDate", "Invalid PassportIssueDate format.");
                existingRecord.PassportIssueDate = date;
            }

            // PassportExpiryDate
            if (payload.ContainsKey("PassportExpiryDate"))
            {
                var val = payload["PassportExpiryDate"]?.ToString();
                if (!DateTime.TryParse(val, out DateTime date)) return (false, "PassportExpiryDate", "Invalid PassportExpiryDate format.");

                var attr = new PassportExpiryDateAttribute();
                var res = attr.GetValidationResult(date, context);
                if (res != ValidationResult.Success) return (false, "PassportExpiryDate", res?.ErrorMessage ?? "Invalid Expiry Date.");
                existingRecord.PassportExpiryDate = date;
            }

            // GivenName
            if (payload.ContainsKey("GivenName"))
            {
                var val = payload["GivenName"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "GivenName", "GivenName cannot be empty.");
                existingRecord.GivenName = val;
            }

            // Surname
            if (payload.ContainsKey("Surname"))
            {
                var val = payload["Surname"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "Surname", "Surname cannot be empty.");
                existingRecord.Surname = val;
            }

            // DateOfBirth
            if (payload.ContainsKey("DateOfBirth"))
            {
                var val = payload["DateOfBirth"]?.ToString();
                if (!DateTime.TryParse(val, out DateTime date)) return (false, "DateOfBirth", "Invalid DateOfBirth format.");

                var attr = new DateOfBirthAttribute();
                var res = attr.GetValidationResult(date, context);
                if (res != ValidationResult.Success) return (false, "DateOfBirth", res?.ErrorMessage ?? "Invalid DateOfBirth.");

                existingRecord.DateOfBirth = date;
            }

            // Gender
            if (payload.ContainsKey("Gender"))
            {
                var val = payload["Gender"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "Gender", "Gender cannot be empty.");

                var attr = new GenderCheckAttribute();
                var res = attr.GetValidationResult(val, context);
                if (res != ValidationResult.Success) return (false, "Gender", res?.ErrorMessage ?? "Invalid Gender.");
                existingRecord.Gender = val;
            }

            // PhoneNo
            if (payload.ContainsKey("PhoneNo"))
            {
                var val = payload["PhoneNo"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "PhoneNo", "PhoneNo cannot be empty.");
                if (!new PhoneAttribute().IsValid(val)) return (false, "PhoneNo", "Invalid Phone format.");
                existingRecord.PhoneNo = val;
            }

            // Email
            if (payload.ContainsKey("Email"))
            {
                var val = payload["Email"]?.ToString();
                if (string.IsNullOrWhiteSpace(val)) return (false, "Email", "Email cannot be empty.");
                if (!new EmailAddressAttribute().IsValid(val)) return (false, "Email", "Invalid Email format.");
                existingRecord.Email = val;
            }

            if (payload.ContainsKey("PassportNo")) existingRecord.PassportNo = payload["PassportNo"]?.ToString() ?? string.Empty;
            if (payload.ContainsKey("MaritalStatus")) existingRecord.MaritalStatus = payload["MaritalStatus"]?.ToString() ?? string.Empty;

            // Database save operation
            var rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0 ? (true, "Success", "Success") : (false, "Failure", "Something went wrong. No changes detected or updated.");
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
