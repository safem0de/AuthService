using System.Text.Json.Serialization;

namespace AuthService.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string access_token { get; set; } = "";
        [JsonPropertyName("refresh_token")]
        public string refresh_token { get; set; } = "";
        [JsonPropertyName("token_type")]
        public string token_type { get; set; } = "";
        [JsonPropertyName("expires_in")]
        public string expires_in { get; set; } = "";
    }
}