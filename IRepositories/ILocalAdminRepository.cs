using AuthService.Models;
using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface ILocalAdminRepository
    {
        Task<ServiceResponse<LocalAdmin>> GetDataFromLDAPAsync (string username);
        Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(string username, string plainPassword);
    }
}