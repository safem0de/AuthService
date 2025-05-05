namespace AuthService.Configurations
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 30;
        public string Issuer { get; set; } = "AuthService";
        public string Audience { get; set; } = "AuthClients";
    }
}