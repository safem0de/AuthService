using System.Net;
using System.Text.Json;
using AuthService.IRepositories;
using AuthService.Models.Dtos;
using AuthService.Services;
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
        [HttpPatch("employee/{id}")]
        public async Task<IActionResult> UpdateUserInfo(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var statusCode = await _apiRepo.UpdateUserInfo(id, dto.Email, dto.Title);
            
            if (statusCode == HttpStatusCode.NoContent)
                return NoContent(); // ✅ สอดคล้องกับ NetSuite
            else
                return StatusCode((int)statusCode);
        }

        [HttpPost("SuiteQL")]
        public async Task<IActionResult> RunSuiteQL([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("q", out var qElement))
                {
                    return BadRequest(new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "❌ Missing 'q' in request body",
                        Data = null!
                    });
                }

                var query = qElement.GetString();

                var result = await _apiRepo.CallSuiteQLAsync(query!);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"❌ Error: {ex.Message}",
                    Data = null!
                });
            }
        }


        [HttpGet("OAuth2customers")]
        public async Task<IActionResult> GetCustomers([FromQuery] string accessToken)
        {
            var result = await _apiRepo.GetCustomersAsync(accessToken);
            return result is not null ? Ok(result) : StatusCode(500, "NetSuite API failed");
        }

        [HttpGet("OAuth2employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] string accessToken)
        {
            var result = await _apiRepo.GetEmployeesAsync(accessToken);
            return result is not null ? Ok(result) : StatusCode(500, "NetSuite API failed");
        }
    }
}