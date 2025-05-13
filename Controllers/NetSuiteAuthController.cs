using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetSuiteAuthController : ControllerBase
    {
        private readonly INetSuiteAuthRepository _authRepo;

        public NetSuiteAuthController(INetSuiteAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }
        [HttpGet("authorize-url")]
        public IActionResult GetAuthorizeUrl()
        {
            var url = _authRepo.BuildAuthorizationUrl();
            return Ok(new { url });
        }


        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Missing code");

            var token = await _authRepo.ExchangeCodeForTokenAsync(code);
            if (token == null)
                return StatusCode(500, "Token exchange failed");

            return Ok(token);
        }
    }
}