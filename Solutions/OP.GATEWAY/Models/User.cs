using System.ComponentModel.DataAnnotations.Schema;

namespace OP.GATEWAY.Models
{
    public static class UserType
    {
        public static string GATEWAY = "GATEWAY";
        public static string AIS = "AIS";
        public static string OVMC = "OVMC";
    }

    [Table("gateway_users")]
    public class ApiUser
    {
        public int Id { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
