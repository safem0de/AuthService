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
            var fullUri = NetSuiteConstants.GetSuiteQLApiBaseUrl(_config["NetSuite:Account"]!);

            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var oauthParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", _config["NetSuite:ConsumerKey"]! },
                { "oauth_token", _config["NetSuite:TokenId"]! },
                { "oauth_nonce", nonce },
                { "oauth_timestamp", timestamp },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_version", "1.0" },
                { "realm", _config["NetSuite:Account"]! }
            };

            // Step 1: Build signature base string
            var signatureBase = $"POST&{Uri.EscapeDataString(fullUri)}&" + Uri.EscapeDataString(string.Join("&", oauthParams
                .OrderBy(x => x.Key)
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")));

            // Step 2: Sign it
            var signingKey = $"{Uri.EscapeDataString(_config["NetSuite:ConsumerSecret"]!)}&{Uri.EscapeDataString(_config["NetSuite:TokenSecret"]!)}";
            using var hasher = new HMACSHA256(Encoding.ASCII.GetBytes(signingKey));
            var signatureBytes = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBase));
            var signature = Convert.ToBase64String(signatureBytes);

            // Step 3: Add signature to params
            oauthParams.Add("oauth_signature", signature);

            // Step 4: Construct Authorization header
            var authHeader = "OAuth " + string.Join(", ", oauthParams.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Replace("OAuth ", ""));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                q = query
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(fullUri, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"❌ Failed: {response.StatusCode}\n{responseBody}");
            }

            return new ServiceResponse<string>
            {
                Data = responseBody,
                Success = true,
                Message = "SuiteQL query executed successfully"
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