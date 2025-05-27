using System.Net;
using AuthService.Models.Dtos;
using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface INetSuiteApiRepository
    {
        /* OAuth1.0 */
        Task<ServiceResponse<object>> CallSuiteQLAsync(string query);

        Task<HttpStatusCode> UpdateUserInfo(int id, string email, string title);

        Task<int> CreateUser(EmployeeDto employeeDto);

        /* OAuth2.0 */
        Task<ServiceResponse<object?>> GetCustomersAsync(string accessToken); 

        Task<ServiceResponse<object?>> GetEmployeesAsync(string accessToken);
    }
}