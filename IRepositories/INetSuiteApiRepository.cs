using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface INetSuiteApiRepository
    {
        Task<ServiceResponse<object?>> GetCustomersAsync(string accessToken);

        Task<ServiceResponse<object?>> GetEmployeesAsync(string accessToken);
    }
}