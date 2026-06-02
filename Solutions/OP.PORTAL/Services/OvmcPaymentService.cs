using Microsoft.EntityFrameworkCore;
using OP.PORTAL.Data;
using OP.PORTAL.Helpers;
using OP.PORTAL.Models;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.Json;
using static MudBlazor.CategoryTypes;

namespace OP.PORTAL.Services
{
    public class OvmcPaymentService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ITokenHelper _appTokenHelper;

        public OvmcPaymentService(IConfiguration config, AppDbContext db, ITokenHelper appTokenHelper, HttpClient httpClient)
        {
            _db = db;
            _config = config;
            _appTokenHelper = appTokenHelper;
            _httpClient = httpClient;
        }

        public async Task<dynamic?> InitiatePaymentAsync(int requestId)
        {
            var orderId = await this.CreatePaymentAsync(requestId);
            var payment = await this.GetPaymentAsync(orderId);
            if (payment == null) return null;

            var merchantId = _config["SmartPay:MerchantId"];
            var workingKey = _config["SmartPay:WorkingKey"];
            var accessCode = _config["SmartPay:AccessCode"];
            var paymentUrl = _config["SmartPay:PaymentUrl"];
            var redirectUrl = _config["SmartPay:PaymentRedirectUrl"];

            string formattedAmount = payment.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            var bank_request_data = string.Empty;
            IDictionary<string, string> requestData = new Dictionary<string, string>()
            {
                { "tid", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() },
                { "merchant_id", merchantId ?? string.Empty },
                { "order_id", payment.OrderNo },
                { "amount", formattedAmount },
                { "currency", "OMR" },
                { "redirect_url", redirectUrl ?? string.Empty },
                { "cancel_url", redirectUrl ?? string.Empty },
                { "language", "EN" },
                { "customer_identifier", "Request_" + payment.RequestId }
            };

            foreach (var item in requestData)
            {
                bank_request_data += item.Key + "=" + WebUtility.UrlEncode(item.Value) + "&";
            }

            var encRequest = new PaymentCryptoHelper().Encrypt(bank_request_data, workingKey ?? string.Empty);
            var paymentData = new
            {
                paymentUrl,
                encRequest,
                accessCode
            };
            return paymentData;
        }

        public async Task<string> CreatePaymentAsync(int requestId)
        {
            int sponserId = await _appTokenHelper.GetSponsorId() ?? 0;
            var request = await _db.OvmcRequests.FirstOrDefaultAsync(x => x.Id == requestId);
            if(request == null) return string.Empty;

            var fee = _config["OvmcFee:" + request.Country];
            decimal amount = Convert.ToDecimal(fee, CultureInfo.InvariantCulture);

            var orderPrefix = _config["SmartPay:OrderNoPrefix"] ?? "OVMC";

            var payment = new OvmcPayment
            {
                RequestId = requestId,
                SponserId = sponserId,
                Amount = amount,
                OrderNo = orderPrefix + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Status = OvmcPaymentStatus.INITIATED,
                CreatedDate = DateTime.Now,
                LastModifiedBy = await _appTokenHelper.GetUsername()
            };

            _db.OvmcPayments.Add(payment);
            int rec = await _db.SaveChangesAsync();

            if (rec > 0)
            {                
                if (request != null)
                {
                    request.PaymentStatus = OvmcPaymentStatus.INITIATED;
                    request.ModifiedDate = DateTime.Now;
                    request.LastModifiedBy = await _appTokenHelper.GetUsername();
                    _db.OvmcRequests.Update(request);
                    await _db.SaveChangesAsync();
                }
            }

            return payment.OrderNo;
        }

        public async Task<OvmcPayment?> GetPaymentAsync(string orderId)
            => await _db.OvmcPayments.AsNoTracking().FirstOrDefaultAsync(d => d.OrderNo.Equals(orderId));

        public async Task<int> UpdateStatusAsync(NameValueCollection responseValues)
        {
            if(responseValues != null)
            {
                var orderId = responseValues["order_id"] ?? string.Empty;
                var status = responseValues["order_status"] ?? string.Empty;

                var payment = await _db.OvmcPayments.FirstOrDefaultAsync(x => x.OrderNo == orderId);
                if (payment != null)
                {
                    payment.Status = status;
                    payment.TrackingId = responseValues["tracking_id"] ?? string.Empty;
                    payment.BankRefNo = responseValues["bank_ref_no"] ?? string.Empty;
                    payment.BankFailureMessage = responseValues["failure_message"] ?? string.Empty;
                    payment.BankPaymentMode = responseValues["payment_mode"] ?? string.Empty;
                    payment.BankCardName = responseValues["card_name"] ?? string.Empty;
                    payment.BankStatusCode = responseValues["status_code"] ?? string.Empty;
                    payment.BankStatusMessage = responseValues["status_message"] ?? string.Empty;
                    payment.BankAmount = decimal.TryParse(responseValues["amount"] ?? string.Empty, out var amt) ? amt : 0;
                    payment.ModifiedDate = DateTime.Now;
                    payment.LastModifiedBy = await _appTokenHelper.GetUsername();

                    int rec = await _db.SaveChangesAsync();

                    if (rec > 0)
                    {
                        var request = await _db.OvmcRequests.FirstOrDefaultAsync(x => x.Id == payment.RequestId);
                        if (request != null)
                        {
                            var ticks = new DateTime(2026, 1, 1).Ticks;
                            var ans = DateTime.Now.Ticks - ticks;
                            var uniqueId = ans.ToString("x").ToUpper();
                            request.OvmcUrnNumber = status.ToLower().Equals("success") ? uniqueId : string.Empty;
                            request.PaymentStatus = status.ToLower().Equals("success") ? OvmcPaymentStatus.SUCCESS : status;
                            request.ModifiedDate = DateTime.Now;
                            request.LastModifiedBy = await _appTokenHelper.GetUsername();
                            await _db.SaveChangesAsync();
                        }
                    }
                    return rec;
                }                
            }
            return 0;            
        }

        public async Task<int> CheckStatusFromSchedulerAsync()
        {
            var workingKey = _config["SmartPay:WorkingKey"] ?? string.Empty;
            var accessCode = _config["SmartPay:AccessCode"] ?? string.Empty;
            var paymentStatusUrl = _config["SmartPay:PaymentStatusUrl"] ?? string.Empty;

            IList<OvmcPayment> updatedPayments = new List<OvmcPayment>();

            var payments = _db.OvmcPayments.AsNoTracking().Where(x => x.Status.Equals(OvmcPaymentStatus.INITIATED)).Take(10);
            foreach (var item in payments)
            {
                var encRequest = new PaymentCryptoHelper().Encrypt(JsonSerializer.Serialize(new { order_no = item.OrderNo }), workingKey);                
                var formData = new Dictionary<string, string>
                {
                    { "enc_request", encRequest },
                    { "access_code", accessCode },
                    { "command", "orderStatusTracker" },
                    { "request_type", "JSON" },
                    { "response_type", "JSON" },
                    { "version", "1.2" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, paymentStatusUrl)
                {
                    Content = new FormUrlEncodedContent(formData)
                };

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var rawResponse = await response.Content.ReadAsStringAsync();

                    var responseDict = System.Web.HttpUtility.ParseQueryString(rawResponse);
                    string api_status = responseDict["status"] ?? string.Empty;
                    string encResponse = responseDict["enc_response"] ?? string.Empty;

                    if (Convert.ToInt16(api_status) == 0)
                    {
                        if (!string.IsNullOrEmpty(encResponse))
                        {
                            var decrypted = new PaymentCryptoHelper().Decrypt(encResponse, workingKey);
                            var values = new NameValueCollection();

                            using var document = JsonDocument.Parse(decrypted);
                            foreach (var property in document.RootElement.EnumerateObject())
                            {
                                values.Add(
                                    property.Name,
                                    property.Value.ValueKind == JsonValueKind.String
                                        ? property.Value.GetString()
                                        : property.Value.ToString()
                                );
                            }

                            item.Status = values["order_status"] ?? string.Empty;
                            item.TrackingId = values["reference_no"] ?? string.Empty;
                            item.BankRefNo = values["order_bank_ref_no"] ?? string.Empty;
                            item.BankCardName = values["order_card_name"] ?? string.Empty;
                            item.BankStatusCode = values["status"] ?? string.Empty;
                            item.BankStatusMessage = values["order_bank_response"] ?? string.Empty;
                            item.BankAmount = decimal.TryParse(values["order_amt"] ?? string.Empty, out var amt) ? amt : 0;
                            item.ModifiedDate = DateTime.Now;
                            item.LastModifiedBy = "Scheduler";

                            updatedPayments.Add(item);
                        }
                    }
                    else
                    {
                        Console.WriteLine(encResponse);
                    }                    
                }
            }

            if (updatedPayments.Count > 0)
            {
                _db.OvmcPayments.UpdateRange(updatedPayments);
                int rec = await _db.SaveChangesAsync();
                foreach (var payment in updatedPayments)
                {
                    var request = await _db.OvmcRequests.FirstOrDefaultAsync(x => x.Id == payment.RequestId);
                    if (request != null)
                    {
                        request.OvmcUrnNumber = OvmcPaymentStatus.IsPaymentSuccess(payment.Status) ? Guid.NewGuid().ToString("N").ToUpper() : string.Empty;
                        request.PaymentStatus = OvmcPaymentStatus.IsPaymentSuccess(payment.Status) ? OvmcPaymentStatus.SUCCESS : !string.IsNullOrEmpty(payment.Status) ? payment.Status : OvmcPaymentStatus.FAILED;
                        request.ModifiedDate = DateTime.Now;
                        await _db.SaveChangesAsync();
                    }
                }
            }
            return 1;
        }
    }
}
