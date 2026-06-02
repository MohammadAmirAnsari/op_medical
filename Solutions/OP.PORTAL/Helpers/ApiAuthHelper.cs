using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Reflection.PortableExecutable;

namespace OP.PORTAL.Helpers
{
    public class ApiSettings
    {
        public string ApiHeaderKey { get; set; } = string.Empty;
        public string ApiHeaderValue { get; set; } = string.Empty;
    }

    public class ApiAuthHelper : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<ApiSettings>>().Value;
            if (!context.HttpContext.Request.Headers.TryGetValue(settings.ApiHeaderKey, out var actualValue))
            {
                context.Result = new UnauthorizedObjectResult(new { error = $"Authentication Failed." });
                return;
            }

            if (string.IsNullOrEmpty(settings.ApiHeaderValue) || !actualValue.Equals(settings.ApiHeaderValue))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Authentication Failed - Invalid key value." });
                return;
            }

            await next();
        }
    }
}
