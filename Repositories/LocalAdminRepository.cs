using System.Text.Json;
using AuthService.Data;
using AuthService.IRepositories;
using AuthService.Models;
using AuthService.Models.Dtos;
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
        private readonly INetSuiteApiRepository _netSuiteApiRepository;

        public LocalAdminRepository(AuthDbContext context, TokenService tokenService, ILdapRepository ldapRepository, INetSuiteApiRepository netSuiteApiRepository)
        {
            _context = context;
            _tokenService = tokenService;
            _ldapRepository = ldapRepository;
            _netSuiteApiRepository = netSuiteApiRepository;
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
                    password
                // adResult.Data.DisplayName,
                // adResult.Data.Email ?? "",
                // adResult.Data.Department ?? "",
                // adResult.Data.Title ?? ""
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

        public async Task SyncNetSuiteIdForAllUsersAsync()
        {
            var users = await _context.LocalAdmins
                          .Where(u => u.IsActive && u.NetSuiteId == -1)
                          .ToListAsync();

            foreach (var user in users)
            {
                try
                {
                    var fullname = user.DisplayName.Replace("TH-BTL", "").Replace(",", "").Trim();
                    var query = $"SELECT id, email FROM employee WHERE LOWER(entityid) = LOWER('{fullname}')";
                    var result = await _netSuiteApiRepository.CallSuiteQLAsync(query);

                    if (!result.Success || result.Data == null)
                    {
                        Console.WriteLine($"❌ NetSuite query failed for {user.Username}");
                        continue;
                    }

                    var json = result.Data.ToString();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var parsed = JsonSerializer.Deserialize<SuiteQLResponse<EmployeeDto>>(json!, options);
                    var employees = parsed?.Items ?? new List<EmployeeDto>();

                    var matched = employees.FirstOrDefault();

                    if (matched != null)
                    {
                        user.NetSuiteId = int.Parse(matched.Id!);
                        user.UpdatedAt = DateTime.UtcNow;

                        // ถ้าเจอ employee แล้วแต่ไม่มี email ให้ update
                        if (string.IsNullOrWhiteSpace(matched.Email))
                        {
                            Console.WriteLine($"🛠 Updating missing email for employee id {matched.Id}");
                            await _netSuiteApiRepository.UpdateUserInfo(user.NetSuiteId, user.Email, user.Title);
                        }

                        Console.WriteLine($"✅ Matched: ID={matched.Id}, Email={matched.Email}");
                    }
                    else
                    {
                        // ✅ ไม่มีพนักงานใน NetSuite → สร้างใหม่
                        Console.WriteLine($"➕ Creating new NetSuite employee for {fullname}");

                        var nameParts = fullname.Split(' ');
                        var firstname = nameParts.ElementAtOrDefault(0) ?? "";
                        var lastname = nameParts.ElementAtOrDefault(1) ?? "";

                        var dto = new EmployeeDto
                        {
                            Firstname = firstname,
                            Lastname = lastname,
                            Email = user.Email,
                            Title = user.Title,
                            Subsidiary = 1
                        };

                        var newId = await _netSuiteApiRepository.CreateUser(dto);
                        if (newId > 0)
                        {
                            user.NetSuiteId = newId;
                            user.UpdatedAt = DateTime.UtcNow;
                            Console.WriteLine($"✅ Created: ID={newId}, Name={firstname} {lastname}");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to create NetSuite employee for {fullname}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error syncing {user.Username}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task<ServiceResponse<LocalAdmin>> SyncUserAfterAdLoginAsync(string username, string password)
        {
            var response = new ServiceResponse<LocalAdmin>();
            try
            {
                var existing = await _context.LocalAdmins
                // .Where(u => u.NetSuiteId > 0)
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

        public async Task<ServiceResponse<string>> SyncUserBeforeAdLoginAsync(List<LdapUserDto> users)
        {
            var response = new ServiceResponse<string>();
            try
            {
                foreach (var user in users)
                {
                    var username = user.Username.ToLower();

                    var exists = await _context.LocalAdmins.AnyAsync(u => u.Username.ToLower() == username);

                    if (!exists)
                    {
                        var newUser = new LocalAdmin
                        {
                            Username = username,
                            DisplayName = user.DisplayName,
                            Email = user.Email!,
                            Department = user.Department!,
                            Title = user.Title!,
                            Role = "User",
                            NetSuiteId = -1,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                            // ❌ ยังไม่ set PasswordHash และ Salt รอ Login
                        };

                        await _context.LocalAdmins.AddAsync(newUser);
                    }
                }

                var inserted = await _context.SaveChangesAsync();
                response.Success = true;
                response.Message = $"✅ Synced {inserted} users into local database.";
                response.Data = $"{inserted} users added.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"❌ Sync error: {ex.Message}";
                response.Data = null!;
            }

            return response;
        }
    }
}