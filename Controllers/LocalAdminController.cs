using AuthService.IRepositories;
using AuthService.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalAdminController : ControllerBase
    {
        private readonly ILocalAdminRepository _repo;

        public LocalAdminController(ILocalAdminRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("login-local")]
        public async Task<IActionResult> LoginLocalAsync([FromBody] LoginData request)
        {
            var result = await _repo.LoginLocalAsync(request.Username, request.Password);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result);
        }
    }
}