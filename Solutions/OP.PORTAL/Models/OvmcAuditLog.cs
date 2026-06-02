using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Models
{
    [Table("portal_audit_logs")]
    public class OvmcAuditLog
    {
        public long Id { get; set; }
        public string TableName { get; set; } = "";
        public string RecordId { get; set; } = "";
        public string Action { get; set; } = "";
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? ChangedBy { get; set; }
        public string? IpAddress { get; set; }
        public DateTime ChangedOn { get; set; } = DateTime.Now;
    }
}
