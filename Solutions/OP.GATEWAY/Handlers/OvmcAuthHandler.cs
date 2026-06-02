using OP.GATEWAY.Helpers;
using OP.GATEWAY.Models;
using OP.GATEWAY.Services;
using System.Net;
using System.Security.Claims;

namespace OP.GATEWAY.Handlers
{
    public class OvmcAuthHandler(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IAuthService authService, ILogService logService) : DelegatingHandler
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IAuthService _authService = authService;
        private readonly ILogService _logService = logService;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = null as HttpResponseMessage;
            try
            {
                string apiRequestId = request.Headers.Contains("op-request-id")
                ? request.Headers.GetValues("op-request-id").FirstOrDefault() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

                string? apiAuthKey = _configuration.GetSection($"DownstreamCredentials:OVMC:AuthKey").Value;
                string? apiAuthValue = _configuration.GetSection($"DownstreamCredentials:OVMC:AuthValue").Value;

                if (string.IsNullOrEmpty(apiAuthKey) || string.IsNullOrEmpty(apiAuthValue))
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Failed to obtain OVMC API token."),
                        ReasonPhrase = "Authentication Failed"
                    };
                }

                request.Headers.Authorization = null;
                request.Headers.Add(apiAuthKey, apiAuthValue);

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
                    uriBuilder.Query = query.ToString();
                    request.RequestUri = uriBuilder.Uri;
                }

                string requestBody = string.Empty;
                if (request.Content != null)
                {
                    requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                }

                response = await base.SendAsync(request, cancellationToken);
                response.Headers.Add("op-request-id", apiRequestId);

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

                _ = Task.Run(async () => await _logService.ApiLog(UserType.AIS, newLog), cancellationToken);
            }
            catch (Exception ex)
            {
                AppLogHelper.WriteLog("Failed to log AIS API request/response : " + ex.Message);
            }

            return response ?? new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Failed to obtain AIS API token."),
                ReasonPhrase = "Authentication Failed"
            };
        }
    }
}
