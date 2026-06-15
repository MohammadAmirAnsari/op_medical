using Microsoft.AspNetCore.Mvc;
using OP.PORTAL.Services;
using System.Threading.Tasks;

namespace OP.PORTAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailAuthController : ControllerBase
    {
        private readonly ISponsorService _sponsorService;

        public EmailAuthController(ISponsorService sponsorService)
        {
            _sponsorService = sponsorService;
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token.");
            }

            bool isVerified = await _sponsorService.VerifyEmailTokenAsync(token);
            if (isVerified)
            {
                return Redirect("/?emailVerified=true");
            }

            return Redirect("/?emailVerified=false");
        }
    }
}
