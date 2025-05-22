using System.Text.Json.Serialization;

namespace AuthService.Models.Dtos
{
    public class EmployeeDto
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}