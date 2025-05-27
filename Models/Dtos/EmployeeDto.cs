using System.Text.Json.Serialization;

namespace AuthService.Models.Dtos
{
    public class EmployeeDto
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("firstname")]
        public string? Firstname { get; set; }
        [JsonPropertyName("lasttname")]
        public string? Lastname { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("subsidiary")]
        public int Subsidiary { get; set; } = 1;
    }
}