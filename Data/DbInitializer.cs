using AuthService.Models;
using AuthService.Utils;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class DbInitializer
    {
        public static void Seeds(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            context.Database.Migrate();

            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        Name = "SuperAdmin",
                        CanCreateUser = true,
                        CanDeleteUser = true,
                        CanViewAllData = true,
                        CanExportReport = true,
                        CanEditData = true,
                        CanViewAuditLog = true
                    },
                    new Role
                    {
                        Name = "Admin",
                        CanCreateUser = true,
                        CanDeleteUser = true,
                        CanViewAllData = true,
                        CanExportReport = true,
                        CanEditData = true,
                        CanViewAuditLog = true
                    },
                    new Role
                    {
                        Name = "Manager",
                        CanCreateUser = false,
                        CanDeleteUser = false,
                        CanViewAllData = false,   // แสดงเฉพาะกลุ่ม: คุม logic ภายนอก
                        CanExportReport = true,
                        CanEditData = false,
                        CanViewAuditLog = false
                    },
                    new Role
                    {
                        Name = "User",
                        CanCreateUser = false,
                        CanDeleteUser = false,
                        CanViewAllData = false,   // เฉพาะตัวเอง
                        CanExportReport = false,
                        CanEditData = true,
                        CanViewAuditLog = false
                    },
                    new Role
                    {
                        Name = "Guest",
                        CanCreateUser = false,
                        CanDeleteUser = false,
                        CanViewAllData = false,   // จำกัด
                        CanExportReport = false,
                        CanEditData = false,
                        CanViewAuditLog = false
                    },
                    new Role
                    {
                        Name = "Auditor",
                        CanCreateUser = false,
                        CanDeleteUser = false,
                        CanViewAllData = true,
                        CanExportReport = true,
                        CanEditData = false,
                        CanViewAuditLog = true
                    },
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }


            if (!context.LocalAdmins.Any())
            {
                var userId = "localadmin";
                var displayName = "System Admin";

                var password = "Admin@123";
                var salt = Guid.NewGuid().ToString("N");
                var passwordHash = PasswordHasher.Hash(password, salt);

                var admin = new LocalAdmin
                {
                    Username = userId,
                    DisplayName = displayName,
                    Email = "admin@example.com",
                    Department = "IT",
                    Title = "System Administrator",
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = "Admin",
                    IsActive = true,
                    NetSuiteId = 0,
                    CreatedAt = DateTime.UtcNow
                };

                context.LocalAdmins.Add(admin);
                context.SaveChanges();
            }
        }
    }
}