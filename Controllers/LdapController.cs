using AuthService.IRepositories;
using AuthService.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LdapController : ControllerBase
    {
        private readonly ILdapRepository _ldapRepository;
        private readonly ILocalAdminRepository _localAdminRepository;

        public LdapController(ILdapRepository ldapRepository, ILocalAdminRepository localAdminRepository)
        {
            _ldapRepository = ldapRepository;
            _localAdminRepository = localAdminRepository;
        }
        
        [HttpPost("token-local")]
        public async Task<IActionResult> TokenLocalAsync([FromBody] LoginData request)
        {
            var result = await _localAdminRepository.LoginAndGenerateTokenAsync(request.Username, request.Password);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result);
        }

        [HttpPost("sync-from-ad")]
        public async Task<IActionResult> SyncFromAdAsync([FromBody] LoginData request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and Password are required.");

            try
            {
                var authResult = await _ldapRepository.AuthenticateAsync(request.Username, request.Password);
                // ✅ Log to console
                Console.WriteLine("========== LDAP Auth Result ==========");
                Console.WriteLine($"Success: {authResult.Success}");
                Console.WriteLine($"Message: {authResult.Message}");
                Console.WriteLine($"DisplayName: {authResult.Data}");
                Console.WriteLine("======================================");

                if (!authResult.Success)
                {
                    return Unauthorized(new { message = authResult.Message });
                }
                else
                {
                    var displayName = authResult.Data!.DisplayName ?? request.Username;
                    var email = authResult.Data.Email ?? string.Empty;
                    var department = authResult.Data.Department ?? string.Empty;
                    var title = authResult.Data.Title ?? string.Empty;

                    _ = await _localAdminRepository.SyncUserAfterAdLoginAsync(
                        request.Username, request.Password, displayName, email, department, title
                    );

                    return Ok(authResult);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}