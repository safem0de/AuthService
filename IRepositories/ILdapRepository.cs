using AuthService.Models.Dtos;
using AuthService.Services;

namespace AuthService.IRepositories
{
    public interface ILdapRepository
    {
        Task<ServiceResponse<LdapUserDto>> AuthenticateAsync(string username, string password);
    }
}