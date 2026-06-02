using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OP.GATEWAY.Models;
using OP.GATEWAY.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OP.GATEWAY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService service, IConfiguration config,
        ILogService logService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IConfiguration _config = config;
        private readonly IAuthService _service = service;
        private readonly ILogService _logService = logService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { success = true, message = "Auth Service is running." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            try
            {
                AuthUserViewModel? user = await _service.ValidateGatewayUserAsync(request);
                if (user == null) return Unauthorized();

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserType", user.UserType)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    success = true,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiry = token.ValidTo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("log")]
        [Authorize(AuthenticationSchemes = "OP.Gateway.Auth")]
        public async Task<IActionResult> Log([FromBody] ApiLogViewModel request)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string username = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? httpContext?.User?.FindFirst("unique_name")?.Value
                ?? "Unknown";

                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = await this._logService.GetApiLogs(request.LogType, username, request)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }
    }
}
