using System.DirectoryServices.Protocols;
using System.Net;
using AuthService.IRepositories;
using AuthService.Models.Dtos;
using AuthService.Services;

namespace AuthService.Repositories
{
    public class LdapRepository : ILdapRepository
    {
        private readonly IConfiguration _configuration;

        public LdapRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // public Task<ServiceResponse<string>> AuthenticateAsync(string username, string password)
        // {
        //     var result = new ServiceResponse<string>();

        //     var ldapServer = _configuration["Ldap:Host"];       // e.g. "ldap.company.com"
        //     int ldapPort = 389;                                 // default fallback
        //     int.TryParse(_configuration["Ldap:Port"], out ldapPort);
        //     var domain = _configuration["Ldap:Domain"];         // e.g. "company.local"
        //     string userDn = $"{domain}\\{username}";

        //     Console.WriteLine(ldapServer);
        //     Console.WriteLine(ldapPort);
        //     Console.WriteLine(domain);
        //     Console.WriteLine(userDn);

        //     try
        //     {
        //         using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, ldapPort));
        //         connection.AuthType = AuthType.Negotiate;

        //         var credential = new NetworkCredential(userDn, password);
        //         connection.Bind(credential); // ⛳ Will throw if invalid

        //         // ค้นหา displayName
        //         var searchRequest = new SearchRequest(
        //             _configuration["Ldap:SearchBase"], // e.g. "DC=company,DC=local"
        //             $"(sAMAccountName={username})",
        //             SearchScope.Subtree,
        //             "displayName"
        //         );

        //         var response = (SearchResponse)connection.SendRequest(searchRequest);
        //         var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();
        //         var displayName = entry?.Attributes["displayName"]?[0]?.ToString() ?? username;

        //         result.Data = displayName;
        //         result.Success = true;
        //         result.Message = "LDAP authentication successful";
        //     }
        //     catch (Exception ex)
        //     {
        //         result.Data = string.Empty;
        //         result.Success = false;
        //         result.Message = $"LDAP authentication failed: {ex.Message}";
        //     }

        //     return Task.FromResult(result);
        // }

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
    }
}