using AuthService.Data;
using AuthService.IRepositories;
using AuthService.Models;
using AuthService.Services;
using AuthService.Utils;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class LocalAdminRepository : ILocalAdminRepository
    {
        private readonly AuthDbContext _context;

        public LocalAdminRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Task<ServiceResponse<LocalAdmin>> GetDataFromLDAPAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(string username, string plainPassword)
        {
            throw new NotImplementedException();
        }
    }
}