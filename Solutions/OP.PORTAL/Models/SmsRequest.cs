using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Models
{
    public class SmsResponse
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; } = default!;
    }

    [Table("portal_sms_logs")]
    public class SmsRequest
    {
        public int Id { get; set; }
        public string PhoneNo { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string OtpFor { get; set; } = default!;
        public string OtpHash { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedTime { get; set; }
    }
}
