using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class LocalAdmin
    {
        [Key]
        public required string Username { get; set; }
        public required string DisplayName { get; set; }
        public string Email { get; set; } = string.Empty;        // <== ✅ เพิ่ม
        public string Department { get; set; } = string.Empty;   // <== ✅ เพิ่ม
        public string Title { get; set; } = string.Empty;        // <== ✅ เพิ่ม
        public string? PasswordHash { get; set; }
        public string? Salt { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public int NetSuiteId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}