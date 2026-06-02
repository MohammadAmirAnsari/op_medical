using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;
using OP.PORTAL.Services;

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
<<<<<<< HEAD
        public async Task<IActionResult> Get(string OvmcUrnOrPassportNumber)
        {
            if(string.IsNullOrEmpty(OvmcUrnOrPassportNumber))
                return BadRequest(new { success = false, message = "Ovmc URN / Passport number Required" });

            var record = await _ovmcRequestService.GetByUrnAsync(OvmcUrnOrPassportNumber);
            return Ok(new { success = record != null, message = record != null ? "Record Found" : "Record Not Found", data = record ?? null });
=======
        public async Task<IActionResult> Get(string ovmcGrnNumber)
        {
            if(string.IsNullOrEmpty(ovmcGrnNumber))
                return BadRequest(new { success = false, message = _localizer["ApiOvmcGrnRequired"] });

            var record = await _ovmcRequestService.GetByGrnAsync(ovmcGrnNumber);
            return Ok(new { success = record != null, message = record != null ? _localizer["ApiRecordFound"] : _localizer["ApiRecordNotFound"], data = record ?? null });
>>>>>>> parent of d224af8 (changes - suggested by priya)
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] OvmcRequestStatusDto request)
        {
<<<<<<< HEAD
            if (request is null || string.IsNullOrEmpty(request.OvmcUrnNumber) || string.IsNullOrEmpty(request.OvmcUrnNumber))
                return BadRequest(new { success = false, message = "Ovmc URN And Status Required" });
=======
            if (request is null || string.IsNullOrEmpty(request.OvmcGrnNumber) || string.IsNullOrEmpty(request.OvmcGrnNumber))
                return BadRequest(new { success = false, message = _localizer["ApiOvmcGrnAndStatusRequired"] });
>>>>>>> parent of d224af8 (changes - suggested by priya)

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
                return Ok(new { success = true, message = _localizer["ApiOvmcRequestUpdateSuccess"] });
            else
                return BadRequest(new { success = false, message = _localizer["ApiOvmcRequestUpdateFailed"] });
        }
    }
}
