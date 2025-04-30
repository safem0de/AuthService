using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class Role
    {
        [Key]
        public required string Name { get; set; } // เช่น "Admin", "User", "Manager" ...
        public bool CanCreateUser { get; set; }
        public bool CanDeleteUser { get; set; }
        public bool CanViewAllData { get; set; }
        public bool CanExportReport { get; set; }
        public bool CanEditData { get; set; }
        public bool CanViewAuditLog { get; set; }
    }
}