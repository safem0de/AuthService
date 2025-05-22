using AuthService.IRepositories;
using AuthService.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalAdminController : ControllerBase
    {
        private readonly ILocalAdminRepository _localAdminRepo;
        private readonly ILdapRepository _ldapRepository;

        public LocalAdminController(ILocalAdminRepository localAdminRepo, ILdapRepository ldapRepository)
        {
            _ldapRepository = ldapRepository;
            _localAdminRepo = localAdminRepo;
        }

        [HttpPost("sync-users")]
        public async Task<IActionResult> SyncUserFromAd()
        {
            var users = await _ldapRepository.GetAllUserAsync();
            var result = await _localAdminRepo.SyncUserBeforeAdLoginAsync(users.Data!);
            return result.Success ? Ok(result) : StatusCode(500, result);
        }

        [HttpPost("login-local")]
        public async Task<IActionResult> LoginLocalAsync([FromBody] LoginData request)
        {
            var result = await _localAdminRepo.LoginLocalAsync(request.Username, request.Password);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result);
        }

        [HttpPost("token-local")]
        public async Task<IActionResult> TokenLocalAsync([FromBody] LoginData request)
        {
            var result = await _localAdminRepo.LoginAndGenerateTokenAsync(request.Username, request.Password);
            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result);
        }

        [HttpPatch("user-patch-NetSuiteId")]
        public async Task<IActionResult> PatchUserAsync()
        {
            await _localAdminRepo.SyncNetSuiteIdForAllUsersAsync();
            return Ok();
        }
    }
}