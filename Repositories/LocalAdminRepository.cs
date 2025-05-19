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
        private readonly TokenService _tokenService;
        private readonly ILdapRepository _ldapRepository;

        public LocalAdminRepository(AuthDbContext context, TokenService tokenService, ILdapRepository ldapRepository)
        {
            _context = context;
            _tokenService = tokenService;
            _ldapRepository = ldapRepository;
        }

        public async Task<ServiceResponse<string>> LoginAndGenerateTokenAsync(string username, string password)
        {
            var response = new ServiceResponse<string>();

            // 1. พยายามล็อกอินจาก local
            var loginResult = await LoginLocalAsync(username, password);

            // 2. ถ้า login ไม่ผ่าน → ลอง sync จาก AD
            if (!loginResult.Success)
            {
                var adResult = await _ldapRepository.AuthenticateAsync(username, password);
                if (!adResult.Success || adResult.Data == null)
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = $"Local login failed: {loginResult.Message} | AD sync failed: {adResult.Message}",
                        Data = string.Empty
                    };
                }

                // 2.1 Sync ข้อมูลจาก AD มา LocalAdmin
                await SyncUserAfterAdLoginAsync(
                    username,
                    password,
                    adResult.Data.DisplayName,
                    adResult.Data.Email ?? "",
                    adResult.Data.Department ?? "",
                    adResult.Data.Title ?? ""
                );

                // 2.2 ล็อกอินใหม่อีกครั้ง
                loginResult = await LoginLocalAsync(username, password);
                if (!loginResult.Success)
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "Login failed even after AD sync.",
                        Data = string.Empty
                    };
                }
            }

            // 3. Generate JWT Token
            var token = _tokenService.GenerateToken(loginResult.Data!, isFallback: true);

            response.Success = true;
            response.Message = "Login successful. Token generated.";
            response.Data = token;
            return response;
        }

        public async Task<ServiceResponse<LocalAdminDto>> LoginLocalAsync(string username, string password)
        {
            var response = new ServiceResponse<LocalAdminDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
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

                var hashed = PasswordHasher.Hash(password, user.Salt);

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

        public async Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(string username, string password, string displayName = "", string email = "", string department = "", string title = "")
        {
            var response = new ServiceResponse<LocalAdmin>();
            try
            {
                var existing = await _context.LocalAdmins.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
                if (existing == null)
                {
                    // สร้าง Salt และ Hash ใหม่
                    var salt = Guid.NewGuid().ToString("N");
                    var hash = PasswordHasher.Hash(password, salt);

                    var newAdmin = new LocalAdmin
                    {
                        Username = username.ToLower(),
                        DisplayName = displayName,
                        Email = email,
                        Department = department,
                        Title = title,
                        PasswordHash = hash,
                        Salt = salt,
                        Role = "User",
                        NetSuiteId = -1,
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
                    var newHash = PasswordHasher.Hash(password, existing.Salt);

                    if (newHash != existing.PasswordHash)
                    {
                        var newSalt = Guid.NewGuid().ToString("N");
                        existing.Salt = newSalt;
                        existing.PasswordHash = PasswordHasher.Hash(password, newSalt);
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