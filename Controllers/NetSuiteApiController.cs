using AuthService.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetSuiteApiController : ControllerBase
    {
        private readonly INetSuiteApiRepository _apiRepo;

        public NetSuiteApiController(INetSuiteApiRepository apiRepo)
        {
            _apiRepo = apiRepo;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers([FromQuery] string accessToken)
        {
            var result = await _apiRepo.GetCustomersAsync(accessToken);
            return result is not null ? Ok(result) : StatusCode(500, "NetSuite API failed");
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] string accessToken)
        {
            var result = await _apiRepo.GetEmployeesAsync(accessToken);
            return result is not null ? Ok(result) : StatusCode(500, "NetSuite API failed");
        }
    }
}