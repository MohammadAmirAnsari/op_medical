using Microsoft.AspNetCore.Mvc;
using OP.PORTAL.Helpers;
using OP.PORTAL.Services;
using System.Web;

namespace OP.PORTAL.Controllers
{
    [Route("[controller]/[action]")]
    public class PaymentController : Controller
    {
        private readonly OvmcPaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly AesEncryptionHelper _aesEncryptionService;

        public PaymentController(IConfiguration config, OvmcPaymentService paymentService, AesEncryptionHelper aesEncryptionService)
        {
            _config = config;
            _paymentService = paymentService;
            _aesEncryptionService = aesEncryptionService;
        }

        [HttpPost]
        public async Task<IActionResult> Completed()
        {
            try
            {
                if (string.IsNullOrEmpty(Request.Form["encResp"]) || string.IsNullOrEmpty(Request.Form["orderNo"]))
                    return BadRequest();
                else
                {
                    var workingKey = _config["SmartPay:WorkingKey"];
                    var encRespValue = Request.Form["encResp"].ToString();
                    string encResponse1 = new PaymentCryptoHelper().Decrypt(encRespValue ?? string.Empty, workingKey ?? string.Empty);
                    var values = HttpUtility.ParseQueryString(encResponse1);

                    if (values != null)
                    {
                        var orderId = values["order_id"] ?? string.Empty;
                        int rec = await _paymentService.UpdateStatusAsync(values);
                        string encId = _aesEncryptionService.EncryptUrl(Convert.ToString(orderId));
                        return Redirect(Url.Content("~/payment-status/" + encId));
                    }
                }                    

                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
