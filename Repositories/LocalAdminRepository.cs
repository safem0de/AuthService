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

                if (string.IsNullOrWhiteSpace(user.Salt) || string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    response.Data = null!;
                    response.Message = "User data incomplete. Please contact admin.";
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
                var existing = await _context.LocalAdmins
                .Where(u => u.NetSuiteId > 0)
                .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));

                if (existing == null)
                {
                    response.Data = null!;
                    response.Success = false;
                    response.Message = "บัญชีนี้ยังไม่ถูกอนุมัติในระบบ กรุณาติดต่อผู้ดูแลระบบเพื่อตรวจสอบ AD หรือ Oracle NetSuite";

                    return response;
                }
                else
                {
                    var shouldUpdatePassword = false;

                    if (string.IsNullOrWhiteSpace(existing.Salt) || string.IsNullOrWhiteSpace(existing.PasswordHash))
                    {
                        // ⚠️ เคย prepare ไว้แต่ยังไม่มี hash/salt → login ครั้งแรก
                        shouldUpdatePassword = true;
                        response.Message = "First time login - setting password hash.";
                    }
                    else
                    {
                        // 👇 ถ้าเคย login แล้ว → เช็คว่า password เปลี่ยนไหม
                        var newHash = PasswordHasher.Hash(password, existing.Salt);
                        if (newHash != existing.PasswordHash)
                        {
                            shouldUpdatePassword = true;
                            response.Message = "Password changed - updating hash.";
                        }
                        else
                        {
                            response.Message = "User re-logged in - updated timestamp.";
                        }
                    }

                    if (shouldUpdatePassword)
                    {
                        var newSalt = Guid.NewGuid().ToString("N");
                        existing.Salt = newSalt;
                        existing.PasswordHash = PasswordHasher.Hash(password, newSalt);
                    }

                    existing.UpdatedAt = DateTime.UtcNow;
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

        public Task<ServiceResponse<LocalAdmin>> SyncUserBeforeAdLoginAsync(string displayName, string email, string department, string title)
        {
            throw new NotImplementedException();
        }
    }
}