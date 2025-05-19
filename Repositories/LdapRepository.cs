using System.DirectoryServices.Protocols;
using System.Net;
using AuthService.IRepositories;
using AuthService.Models.Dtos;
using AuthService.Services;
using AuthService.Constants;

namespace AuthService.Repositories
{
    public class LdapRepository : ILdapRepository
    {
        private readonly IConfiguration _configuration;

        public LdapRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<ServiceResponse<LdapUserDto>> AuthenticateAsync(string username, string password)
        {
            var result = new ServiceResponse<LdapUserDto>();

            string ldapServer = _configuration["Ldap:Host"]!;
            int ldapPort = 389;
            int.TryParse(_configuration["Ldap:Port"], out ldapPort);

            string userPrincipalName = username.Contains('@') ? username : $"{username}@{_configuration["Ldap:UPNSuffix"]}";
            string searchBase = _configuration["Ldap:SearchBase"] ?? throw new Exception("Ldap:SearchBase not configured");

            string[] requestedAttributes = {
                "displayName", "mail", "department", "title", "memberOf"
            };

            try
            {
                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, ldapPort));
                connection.AuthType = AuthType.Negotiate;

                var credential = new NetworkCredential(userPrincipalName, password);
                connection.Bind(credential); // Will throw if invalid

                Console.WriteLine("✅ LDAP Bind Succeeded");

                // Build search filter
                string filter = $"(userPrincipalName={userPrincipalName})";

                var searchRequest = new SearchRequest(
                    searchBase,
                    filter,
                    SearchScope.Subtree,
                    requestedAttributes
                );

                var response = (SearchResponse)connection.SendRequest(searchRequest);
                var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();

                if (entry == null)
                {
                    result.Success = false;
                    result.Message = "User not found in LDAP";
                    return Task.FromResult(result);
                }

                // Log attributes (optional)
                string displayName = entry.Attributes["displayName"]?[0]?.ToString() ?? username;
                string mail = entry.Attributes["mail"]?[0]?.ToString() ?? "";
                string department = entry.Attributes["department"]?[0]?.ToString() ?? "";
                string title = entry.Attributes["title"]?[0]?.ToString() ?? "";

                Console.WriteLine($"Name: {displayName}");
                Console.WriteLine($"Email: {mail}");
                Console.WriteLine($"Department: {department}");
                Console.WriteLine($"Title: {title}");

                result.Success = true;
                result.Data = new LdapUserDto
                {
                    Username = username,
                    DisplayName = displayName,
                    Email = mail,
                    Department = department,
                    Title = title,
                };
                result.Message = "LDAP authentication successful";
            }
            catch (LdapException ex)
            {
                result.Success = false;
                result.Data = null!;
                result.Message = $"LDAP error: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Data = null!;
                result.Message = $"General error: {ex.Message}";
            }

            return Task.FromResult(result);
        }

        public Task<ServiceResponse<List<LdapUserDto>>> GetAllUserAsync()
        {
            var result = new ServiceResponse<List<LdapUserDto>>();
            var data = new List<LdapUserDto>();
            var _excludeWords = NetSuiteConstants.excludeWords;

            // ✅ ข้อมูลเชื่อมต่อ LDAP
            string ldapServer = _configuration["Ldap:Server"]!;
            string username = _configuration["Ldap:User"]!; // UPN format
            string password = _configuration["Ldap:Pass"]!;
            string searchBase = _configuration["Ldap:SearchBase"]!;

            try
            {
                // ✅ ตั้งค่า LDAP Connection
                var credential = new NetworkCredential(username, password);
                var connection = new LdapConnection(ldapServer)
                {
                    AuthType = AuthType.Negotiate, // หรือ Basic
                    Credential = credential,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                connection.Bind(); // 🔐 Login

                // ✅ ค้นหา user objects
                var request = new SearchRequest(
                    searchBase,
                    // "(objectClass=user)",
                    "(&(objectCategory=person)(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                    SearchScope.Subtree,
                    new[] { "sAMAccountName", "displayName", "department", "mail", "title" }
                );

                var response = (SearchResponse)connection.SendRequest(request);
                foreach (SearchResultEntry entry in response.Entries)
                {
                    var sam = entry.Attributes["sAMAccountName"]?[0]?.ToString();
                    var display = entry.Attributes["displayName"]?[0]?.ToString();
                    var department = entry.Attributes["department"]?[0]?.ToString();
                    var email = entry.Attributes["mail"]?[0]?.ToString();
                    var title = entry.Attributes["title"]?[0]?.ToString();

                    if (string.IsNullOrEmpty(sam) ||
                        _excludeWords.Any(ex => sam.Contains(ex, StringComparison.OrdinalIgnoreCase)) ||
                        string.IsNullOrEmpty(display) ||
                        string.IsNullOrEmpty(email) ||
                        string.IsNullOrEmpty(title) ||
                        _excludeWords.Any(ex => title.Contains(ex, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine(
                            $"⛔ Skipped: {sam} | {display} | {email} | {department} | {title}");
                        continue;
                    }

                    var userItem = new LdapUserDto
                    {
                        Username = sam.ToLower(),
                        DisplayName = display,
                        Email = email,
                        Department = department,
                        Title = title,
                    };

                    data.Add(userItem);
                }

                connection.Dispose();

                result.Data = data.OrderBy(d => d.DisplayName).ToList();
                result.Message = $"Get All Users ({data.Count}) LDAP Success";
                result.Success = true;

            }
            catch (LdapException ex)
            {
                var _msg = $"❌ LDAP Error: {ex.Message}";
                Console.WriteLine(_msg);

                result.Message = _msg;
                result.Success = false;
            }
            catch (Exception ex)
            {
                var _msg = $"❌ General Error: {ex.Message}";
                Console.WriteLine(_msg);

                result.Message = _msg;
                result.Success = false;
            }

            return Task.FromResult(result);
        }
    }
}