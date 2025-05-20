using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface INetSuiteApiRepository
    {
        /* OAuth1.0 */
        Task<ServiceResponse<string>> CallSuiteQLAsync(string query);
        /* OAuth2.0 */
        Task<ServiceResponse<object?>> GetCustomersAsync(string accessToken); 

        Task<ServiceResponse<object?>> GetEmployeesAsync(string accessToken);
    }
}