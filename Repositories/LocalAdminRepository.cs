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

        public async Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(string username, string plainPassword)
        {
            var response = new ServiceResponse<LocalAdmin>();

            try
            {
                var existing = await _context.LocalAdmins.FirstOrDefaultAsync(u => u.UserId == username);

                if (existing == null)
                {
                    // สร้าง Salt และ Hash ใหม่
                    var salt = Guid.NewGuid().ToString("N");
                    var hash = PasswordHasher.Hash(plainPassword, salt);

                    var newAdmin = new LocalAdmin
                    {
                        UserId = username,
                        DisplayName = username,
                        PasswordHash = hash,
                        Salt = salt,
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.LocalAdmins.Add(newAdmin);
                    response.Data = newAdmin;
                    response.Message = "New local admin created from AD login.";
                }
                else
                {
                    // ถ้า password เปลี่ยน ให้ update hash และ salt
                    var newHash = PasswordHasher.Hash(plainPassword, existing.Salt);

                    if (newHash != existing.PasswordHash)
                    {
                        var newSalt = Guid.NewGuid().ToString("N");
                        existing.Salt = newSalt;
                        existing.PasswordHash = PasswordHasher.Hash(plainPassword, newSalt);
                        existing.UpdatedAt = DateTime.UtcNow;
                        response.Message = "Existing local admin updated with new password hash.";
                    }
                    else
                    {
                        existing.UpdatedAt = DateTime.UtcNow;
                        response.Message = "Local admin login successful. No password change.";
                    }

                    _context.LocalAdmins.Update(existing);
                    response.Data = existing;
                }

                await _context.SaveChangesAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error syncing local admin: {ex.Message}";
                response.Data = null!;
            }

            return response;
        }
    }
}