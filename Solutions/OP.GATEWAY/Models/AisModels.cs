using System.Xml.Serialization;

namespace OP.GATEWAY.Models
{
    [XmlRoot("response")]
    public class AuthResponse
    {
        [XmlElement("message")]
        public string Message { get; set; } = string.Empty;

        [XmlElement("status")]
        public string Status { get; set; } = string.Empty;
    }
}
