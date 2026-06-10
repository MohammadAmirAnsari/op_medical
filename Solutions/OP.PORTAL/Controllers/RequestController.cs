using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;
using OP.PORTAL.Services;
using System.Text.Json.Nodes;

namespace OP.PORTAL.Controllers
{
    [ApiAuthHelper]
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IStringLocalizer<RequestController> _localizer;
        private readonly IOvmcRequestService _ovmcRequestService;

        public RequestController(IOvmcRequestService ovmcRequestService, IStringLocalizer<RequestController> localizer)
        {
            _ovmcRequestService = ovmcRequestService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string OvmcUrnOrPassportNumber)
        {
            if(string.IsNullOrEmpty(OvmcUrnOrPassportNumber))
                return BadRequest(new { success = false, message = "Ovmc URN / Passport number Required" });

            var record = await _ovmcRequestService.GetByUrnAsync(OvmcUrnOrPassportNumber);
            return Ok(new { success = record != null, message = record != null ? "Record Found" : "Record Not Found", data = record ?? null });
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] OvmcRequestStatusDto request)
        {
            if (request is null || string.IsNullOrEmpty(request.OvmcUrnNumber) || string.IsNullOrEmpty(request.OvmcUrnNumber))
                return BadRequest(new { success = false, message = "Ovmc URN And Status Required" });

            request.LastModifiedBy = request.LastModifiedBy ?? "API_USER";
            var result = await _ovmcRequestService.UpdateByUrnAsync(request);
            if (result > 0)
                return Ok(new { success = true, message = "Ovmc Request Updated Successfully." });
            else
                return BadRequest(new { success = false, message = "Ovmc Request Update Failed." });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateData(string OvmcUrnOrPassportNumber,[FromBody] OvmcRequestDto request)
        {
            if (request is null || string.IsNullOrEmpty(OvmcUrnOrPassportNumber) || string.IsNullOrEmpty(OvmcUrnOrPassportNumber))
                return BadRequest(new { success = false, message = "Ovmc URN And Status Required" });

            

            var result = await _ovmcRequestService.UpdateByUrnDataAsync(OvmcUrnOrPassportNumber, request);
            if (result > 0)
                return Ok(new { success = true, message = "Ovmc Request Updated Successfully." });
            else
                return BadRequest(new { success = false, message = "Ovmc Request Update Failed." });
        }

        [HttpPatch("UpdateData")]
        public async Task<IActionResult> UpdateUserData([FromBody] JsonObject payload)
        {
            if (payload is null || !payload.ContainsKey("OvmcUrnNumber"))
            {
                ModelState.AddModelError("OvmcUrnNumber", "This field must not be empty.");
                return ValidationProblem(ModelState);
            }

            if(payload.Count == 1)
            {
                return Ok(new { success = true, message = "No records passed to update" });
            }

            var (isValid, key, errorMessage) = await _ovmcRequestService.PatchByUrnDataAsync_New(payload);

            if (!isValid)
            {
                ModelState.AddModelError(key, errorMessage);
                return ValidationProblem(ModelState);
            }

            return Ok(new { success = true, message = "Ovmc Request Updated Successfully." });
        }

    }
}
