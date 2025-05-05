using AuthService.Models;
using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface ILocalAdminRepository
    {
        Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(
            string username, string plainPassword, string displayName, string email, string department, string title);
        Task<ServiceResponse<LocalAdminDto>> LoginLocalAsync(string username, string plainPassword);
    }
}