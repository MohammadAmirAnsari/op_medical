namespace OP.PORTAL.Models
{
    public class OvmcRequestDto
    {
        public string SponserName { get; set; } = string.Empty;
        public string SponsorPhoneNo { get; set; } = string.Empty;
        public string SponsorCity { get; set; } = string.Empty;
        public string SponsorType { get; set; } = string.Empty;
        public string SponsorEmail { get; set; } = string.Empty;
        public string VisaType { get; set; } = string.Empty;
        public string OvmcUrnNumber { get; set; } = string.Empty;
        public string MolWorkPermitNo { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PassportNo { get; set; } = string.Empty;
        public string PassportIssuePlace { get; set; } = string.Empty;
        public DateTime? PassportIssueDate { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
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
