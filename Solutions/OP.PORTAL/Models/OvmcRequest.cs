using OP.PORTAL.Locales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Models
{
    [Table("portal_requests")]
    public class OvmcRequest : Auditable
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public int SponserId { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string VisaType { get; set; } = string.Empty;

        public string OvmcUrnNumber { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]        
        public string MolWorkPermitNo { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PassportNo { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PassportIssuePlace { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public DateTime? PassportIssueDate { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public DateTime? PassportExpiryDate { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string GivenName { get; set; } = string.Empty;

        public string? Surname { get; set; }
                
        public DateTime? DateOfBirth { get; set; }
         
        public string? Gender { get; set; }
         
        public string? MaritalStatus { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; } = string.Empty;

        public string? PhoneNo2 { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public bool IsTnCAccepted { get; set; } = false;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string RequestStatus { get; set; } = OvmcRequestStatus.SUBMITTED;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PaymentStatus { get; set; } = OvmcPaymentStatus.PENDING;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

    }
}
