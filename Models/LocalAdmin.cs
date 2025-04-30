using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
public class LocalAdmin
{
    [Key]
    public required string UserId { get; set; }  

    public string PasswordHash { get; set; } = string.Empty;

    public string Salt { get; set; } = string.Empty;

    public required string DisplayName { get; set; }

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
}