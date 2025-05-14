

using System.Net.Http.Headers;
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