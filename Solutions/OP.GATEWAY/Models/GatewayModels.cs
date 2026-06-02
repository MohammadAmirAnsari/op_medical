using System.ComponentModel.DataAnnotations;

namespace OP.GATEWAY.Models
{
    public record LoginViewModel
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public record AuthUserViewModel
    {
        public string Username { get; set; } = default!;
        public string UserType { get; set; } = default!;
    }

    public class ApiLogViewModel
    {
        [Required(ErrorMessage = "Required value : AIS, DEFAULT ")]
        public string LogType { get; set; } = default!;

        public string RequestId { get; set; } = string.Empty;

        public string SearchInPath { get; set; } = string.Empty;

        public string SearchInRequest { get; set; } = string.Empty;
    }
}
