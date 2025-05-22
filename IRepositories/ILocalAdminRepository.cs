using AuthService.Models;
using AuthService.Models.Dtos;
using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface ILocalAdminRepository
    {
        Task<ServiceResponse<string>> SyncUserBeforeAdLoginAsync(List<LdapUserDto> users);
        Task SyncNetSuiteIdForAllUsersAsync();
        Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(
            string username, string plainPassword);
        Task<ServiceResponse<LocalAdminDto>> LoginLocalAsync(string username, string password);
        Task<ServiceResponse<string>> LoginAndGenerateTokenAsync(string username, string password);
    }
}