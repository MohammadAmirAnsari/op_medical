using OP.PORTAL.Locales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Models
{
    [Table("portal_sponsors")]
    public class Sponsor : Auditable
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string SponsorType { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public bool IsVerified { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }
    }

    public class SponsorRegister : Sponsor
    {        

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [Compare("PasswordHash", ErrorMessageResourceName = "ResxConfirmPassword", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string VerifyOtp { get; set; } = string.Empty;
    }

    public class SponsorLogin
    {
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string VerifyOtp { get; set; } = string.Empty;
    }

    public class SponsorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string SponsorType { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public bool IsVerified { get; set; } = false;
    }

    public class SponsorResetPassword
    {
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        [Compare("Password", ErrorMessageResourceName = "ResxConfirmPassword", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class SponsorForgotPassword : SponsorResetPassword
    {
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string VerifyOtp { get; set; } = string.Empty;
    }
}
