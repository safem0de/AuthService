namespace AuthService.Models.Dtos
{
    public class LdapUserDto
    {
        public string Username { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
    }
}