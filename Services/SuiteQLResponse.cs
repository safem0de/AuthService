using System.Text.Json.Serialization;
using AuthService.Models.Dtos;

namespace AuthService.Services
{
    public class SuiteQLResponse<T>
    {
        [JsonPropertyName("links")]
        public List<object>? Links { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("hasMore")]
        public bool HasMore { get; set; }

        [JsonPropertyName("items")]
        public List<T>? Items { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }
    }
}