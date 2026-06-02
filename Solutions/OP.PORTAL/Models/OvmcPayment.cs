using OP.PORTAL.Locales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Models
{
    [Table("portal_payments")]
    public class OvmcPayment : Auditable
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string OrderNo { get; set; } = null!;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public int SponserId { get; set; }
        
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public int RequestId { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public decimal Amount { get; set; } = 0;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Status { get; set; } = null!;

        public string? TrackingId { get; set; }

        public string? BankRefNo { get; set; }
        public string? BankFailureMessage { get; set; }
        public string? BankPaymentMode { get; set; }
        public string? BankCardName { get; set; }
        public string? BankStatusCode { get; set; }
        public string? BankStatusMessage { get; set; }
        public decimal? BankAmount { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public DateTime? ModifiedDate { get; set; } = DateTime.Now;
    }
}
