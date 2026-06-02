using OP.PORTAL.Locales;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OP.PORTAL.Models
{
    public class OvmcRequestDto
    {
        public string SponserName { get; set; } = string.Empty;
        public string SponsorPhoneNo { get; set; } = string.Empty;
        public string SponsorCity { get; set; } = string.Empty;
        public string SponsorType { get; set; } = string.Empty;
        public string SponsorEmail { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string VisaType { get; set; } = string.Empty;
        public string OvmcUrnNumber { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [OnlyNumber(8, 8)]
        public string MolWorkPermitNo { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string City { get; set; } = string.Empty;
        public string PassportNo { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [NoArabic()]
        public string PassportIssuePlace { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public DateTime? PassportIssueDate { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [PassportExpiryDate()]
        public DateTime? PassportExpiryDate { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string GivenName { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Surname { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [DateOfBirth()]
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [GenderCheck()]
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [Phone()]
        public string PhoneNo { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress()]
        public string Email { get; set; } = string.Empty;
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
