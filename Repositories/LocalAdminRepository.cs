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

        public async Task<ServiceResponse<LocalAdminDto>> LoginLocalAsync(string username, string plainPassword)
        {
            var response = new ServiceResponse<LocalAdminDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(plainPassword))
                {
                    response.Data = null!;
                    response.Message = "Username and password are required.";
                    response.Success = false;

                    return response;
                }

                var user = await _context.LocalAdmins
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()) && u.IsActive);

                if (user == null)
                {
                    response.Data = null!;
                    response.Message = "Invalid username or account is inactive.";
                    response.Success = false;

                    return response;
                }

                var hashed = PasswordHasher.Hash(plainPassword, user.Salt);

                if (hashed != user.PasswordHash)
                {
                    response.Data = null!;
                    response.Message = "Incorrect password.";
                    response.Success = false;

                    return response;
                }
                else
                {
                    response.Data = new LocalAdminDto
                    {
                        Username = user.Username,
                        DisplayName = user.DisplayName,
                        Email = user.Email,
                        Department = user.Department,
                        Title = user.Title,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    };

                    response.Message = "Login successful (local fallback).";
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error local login : {ex.Message}";
                response.Data = null!;
            }

            return response;
        }

        public async Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(string username, string plainPassword, string displayName = "", string email = "", string department = "", string title = "")
        {
            var response = new ServiceResponse<LocalAdmin>();

            try
            {
                var existing = await _context.LocalAdmins.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));

                if (existing == null)
                {
                    // สร้าง Salt และ Hash ใหม่
                    var salt = Guid.NewGuid().ToString("N");
                    var hash = PasswordHasher.Hash(plainPassword, salt);

                    var newAdmin = new LocalAdmin
                    {
                        Username = username,
                        DisplayName = displayName,
                        Email = email,
                        Department = department,
                        Title = title,
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

                        Console.WriteLine(response.Message);
                    }
                    else
                    {
                        existing.UpdatedAt = DateTime.UtcNow;
                        response.Message = "Local admin login successful. No password change.";

                        Console.WriteLine(response.Message);
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