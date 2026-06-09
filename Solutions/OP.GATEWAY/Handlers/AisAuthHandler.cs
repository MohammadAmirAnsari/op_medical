using OP.GATEWAY.Helpers;
using OP.GATEWAY.Models;
using OP.GATEWAY.Services;
using OP.GATEWAY.Data;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;

namespace OP.GATEWAY.Handlers
{
    public class AisAuthHandler(IHttpContextAccessor httpContextAccessor, IAuthService authService, ILogService logService, IServiceProvider serviceProvider) : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IAuthService _authService = authService;
        private readonly ILogService _logService = logService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;


        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string apiRequestId = request.Headers.Contains("op-request-id")
                ? request.Headers.GetValues("op-request-id").FirstOrDefault() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            string? apiToken = await _authService.ValidateDownstreamUserAsync(UserType.AIS);

            if (string.IsNullOrEmpty(apiToken))
            {
                throw new UnauthorizedAccessException("Failed to obtain AIS API token.");
            }

            request.Headers.Authorization = null;

            var httpContext = _httpContextAccessor.HttpContext;

            var username = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? httpContext?.User?.FindFirst("unique_name")?.Value
                ?? "Unknown";

            var ip = httpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? httpContext?.Connection?.RemoteIpAddress?.ToString();

            if (request.RequestUri != null)
            {
                var uriBuilder = new UriBuilder(request.RequestUri ?? new Uri(""));
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["auth_key"] = apiToken;
                uriBuilder.Query = query.ToString();
                request.RequestUri = uriBuilder.Uri;
            }

            string requestBody = string.Empty;
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("op-request-id", apiRequestId);

            try
            {
                var responseBody = string.Empty;
                if (response.Content != null)
                {
                    responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    response.Content = new StringContent(responseBody);
                }

                var newLog = new AisApiLog
                {
                    RequestId = apiRequestId,
                    Username = username,
                    IpAddress = ip ?? string.Empty,
                    Path = request?.RequestUri?.AbsoluteUri ?? string.Empty,
                    Method = request?.Method?.Method ?? string.Empty,
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    StatusCode = response != null ? (int)response.StatusCode : 0,
                    LogTime = DateTime.UtcNow
                };

                var jsonDoc = JsonNode.Parse(requestBody);
                string MedicalStatus = jsonDoc?["status"]?.ToString() ?? string.Empty;
                string OvmcUrnNumber = jsonDoc?["gCCSlipNo"]?.ToString() ?? string.Empty;

                _ = Task.Run(async () =>
                {
                    await _logService.ApiLog(UserType.AIS, newLog);

                    // update medical status
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

                    string sqlQuery = "UPDATE `portal_requests` SET `MedicalStatus` = {0} WHERE `OvmcUrnNumber` = {1} ORDER BY `Id` DESC LIMIT 1;";

                    await dbContext.Database.ExecuteSqlRawAsync(sqlQuery, MedicalStatus, OvmcUrnNumber);

                }, cancellationToken);
            }
            catch (Exception ex)
            {
                AppLogHelper.WriteLog("Failed to log AIS API request/response : " + ex.Message);
            }

            return response ?? throw new UnauthorizedAccessException("Failed to obtain AIS API token.");
        }
    }
}
