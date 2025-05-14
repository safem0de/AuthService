using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using AuthService.Constants;
using AuthService.IRepositories;
using AuthService.Models;

namespace AuthService.Repositories
{
    public class NetSuiteAuthRepository : INetSuiteAuthRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public NetSuiteAuthRepository(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public string BuildAuthorizationUrl(string? customState = null)
        {
            string account = _config["NetSuite:Account"]!;
            string clientId = _config["NetSuite:ConsumerKey"]!;
            string redirectUri = _config["NetSuite:CallbackUrl"]!;
            string responseType = _config["NetSuite:AuthorizationSettings:ResponseType"] ?? "code";
            string scope = _config["NetSuite:AuthorizationSettings:Scope"] ?? "rest_webservices";
            string state = customState ?? _config["NetSuite:AuthorizationSettings:State"] ?? "state123";

            string baseUrl = string.Format(NetSuiteConstants.AuthorizationUrlFormat, account);

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["response_type"] = responseType;
            queryParams["client_id"] = clientId;
            queryParams["redirect_uri"] = redirectUri;
            queryParams["scope"] = scope;
            queryParams["state"] = state;

            return $"{baseUrl}?{queryParams}";
        }

        public async Task<TokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            string account = _config["NetSuite:Account"]!;
            string clientId = _config["NetSuite:ConsumerKey"]!;
            string clientSecret = _config["NetSuite:ConsumerSecret"]!;
            string redirectUri = _config["NetSuite:CallbackUrl"]!;

            // ⚠️ สร้าง absolute URL จาก constant
            string tokenUrl = string.Format(NetSuiteConstants.TokenUrlFormat, account);

            // 🧠 Basic Auth Header
            string authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            });

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ HTTP request failed:");
                Console.WriteLine(ex);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Token exchange failed ({response.StatusCode}):");
                Console.WriteLine(responseJson);
                return null;
            }

            Console.WriteLine("✅ Token response received:");
            Console.WriteLine(responseJson);

            try
            {
                return JsonSerializer.Deserialize<TokenResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine("❌ Failed to deserialize token response:");
                Console.WriteLine(jsonEx);
                return null;
            }
        }
    }
}