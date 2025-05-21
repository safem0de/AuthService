using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuthService.Constants;
using AuthService.IRepositories;
using AuthService.Services;

namespace AuthService.Repositories
{
    public class NetSuiteApiRepository : INetSuiteApiRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public NetSuiteApiRepository(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<ServiceResponse<string>> CallSuiteQLAsync(string query)
        {
            var accountId = _config["NetSuite:Account"]!;
            var consumerKey = _config["NetSuite:ConsumerKey"]!;
            var consumerSecret = _config["NetSuite:ConsumerSecret"]!;
            var tokenId = _config["NetSuite:TokenId"]!;
            var tokenSecret = _config["NetSuite:TokenSecret"]!;

            var baseUrl = $"https://{accountId}.suitetalk.api.netsuite.com/services/rest/query/v1/suiteql";

            // 🔐 OAuth params
            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signatureMethod = "HMAC-SHA256";
            var version = "1.0";

            // 🔧 Sorted dictionary for signature base string
            var parameters = new SortedDictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_token", tokenId },
            { "oauth_nonce", nonce },
            { "oauth_timestamp", timestamp },
            { "oauth_signature_method", signatureMethod },
            { "oauth_version", version },
            { "realm", accountId }
        };

            // 🔐 Generate signature base string
            var encodedParams = string.Join("&", parameters
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            var signatureBaseString = $"POST&{Uri.EscapeDataString(baseUrl)}&{Uri.EscapeDataString(encodedParams)}";

            // 🔐 Generate signing key and signature
            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret)}";
            using var hasher = new HMACSHA256(Encoding.ASCII.GetBytes(signingKey));
            var signatureBytes = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
            var signature = Convert.ToBase64String(signatureBytes);

            // ➕ Add signature to parameters
            parameters.Add("oauth_signature", signature);

            // 🛂 Create Authorization header (NO Uri.Escape on key/value here!)
            var authHeader = "OAuth " + string.Join(", ", parameters
                .Select(kvp => $"{kvp.Key}=\"{Uri.EscapeDataString(kvp.Value)}\""));

            // 🚀 Send HTTP request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Replace("OAuth ", ""));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Prefer", "transient");

            var payload = new { q = query };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"❌ Failed: {response.StatusCode}\n{responseBody}",
                    Data = null!
                };
            }

            return new ServiceResponse<string>
            {
                Success = true,
                Message = "✅ SuiteQL query success",
                Data = responseBody
            };
        }

        public async Task<ServiceResponse<object?>> GetCustomersAsync(string accessToken)
        {
            string account = _config["NetSuite:Account"]!;
            string url = NetSuiteConstants.GetCustomersUrl(account);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during API call: {ex.Message}");
                return new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Exception while calling NetSuite API.",
                    Data = null
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ API call failed ({response.StatusCode}): {responseBody}");
                return new ServiceResponse<object?>
                {
                    Success = false,
                    Message = $"API call failed with status code: {response.StatusCode}",
                    Data = null
                };
            }

            Console.WriteLine("✅ API call successful:");
            Console.WriteLine(responseBody);

            return new ServiceResponse<object?>
            {
                Success = true,
                Message = "Retrieved customers successfully.",
                Data = JsonSerializer.Deserialize<object>(responseBody)
            };
        }

        public async Task<ServiceResponse<object?>> GetEmployeesAsync(string accessToken)
        {
            string account = _config["NetSuite:Account"]!;
            string url = NetSuiteConstants.GetEmployeesUrl(account);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during API call: {ex.Message}");
                return new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Exception while calling NetSuite API.",
                    Data = null
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ API call failed ({response.StatusCode}): {responseBody}");
                return new ServiceResponse<object?>
                {
                    Success = false,
                    Message = $"API call failed with status code: {response.StatusCode}",
                    Data = null
                };
            }

            Console.WriteLine("✅ API call successful:");
            Console.WriteLine(responseBody);

            return new ServiceResponse<object?>
            {
                Success = true,
                Message = "Retrieved employees successfully.",
                Data = JsonSerializer.Deserialize<object>(responseBody)
            };
        }
    }
}