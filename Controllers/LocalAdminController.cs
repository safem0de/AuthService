using AuthService.IRepositories;
using AuthService.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalAdminController : ControllerBase
    {
        private readonly ILdapRepository _ldapRepository;

        public LocalAdminController(ILdapRepository ldapRepository)
        {
            _ldapRepository = ldapRepository;
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
                    return Ok(authResult);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // [HttpPost("/api/test")]
        // public IActionResult Debug([FromBody] LocalAdminDto request)
        // {
        //     return Ok($"✅ Route work!" {request.ToString()});
        // }
    }
}