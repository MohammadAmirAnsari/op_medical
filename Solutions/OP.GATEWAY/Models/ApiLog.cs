using System.ComponentModel.DataAnnotations.Schema;

namespace OP.GATEWAY.Models
{
    public class ApiLog
    {
        public int Id { get; set; }
        public string RequestId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime LogTime { get; set; }
    }

    [Table("gateway_ais_logs")]
    public class AisApiLog : ApiLog { }
}
