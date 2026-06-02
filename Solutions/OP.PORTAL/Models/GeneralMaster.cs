using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Models
{
    [Table("portal_general_master")]
    public class GeneralMaster
    {
        public int Id { get; set; }
        public string MasterType { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
