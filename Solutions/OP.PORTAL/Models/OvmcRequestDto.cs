using OP.PORTAL.Locales;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OP.PORTAL.Models
{
    public class OvmcRequestDto
    {
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string OvmcUrnNumber { get; set; } = string.Empty;
        public string? SponserName { get; set; }
        public string? SponsorPhoneNo { get; set; }
        public string? SponsorCity { get; set; }
        public string? SponsorType { get; set; }
        public string? SponsorEmail { get; set; }
        public string? VisaType { get; set; }
        public string? MolWorkPermitNo { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PassportNo { get; set; }
        public string? PassportIssuePlace { get; set; }
        public DateTime? PassportIssueDate { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? PhoneNo { get; set; }
        public string? Email { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class OvmcRequestStatusDto : Auditable
    {
        public string OvmcUrnNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
