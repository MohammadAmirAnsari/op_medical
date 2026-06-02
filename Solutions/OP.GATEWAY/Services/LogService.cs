using Microsoft.EntityFrameworkCore;
using OP.GATEWAY.Data;
using OP.GATEWAY.Models;

namespace OP.GATEWAY.Services
{
    public interface ILogService
    {
        Task ApiLog(string type, dynamic log);

        Task<IList<ApiLog>> GetApiLogs(string type, string username, ApiLogViewModel model);
    }

    public class LogService : ILogService
    {
        private readonly IServiceProvider _serviceProvider;

        public LogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ApiLog(string type, dynamic log)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
                if (type == UserType.AIS) await dbContext.AisApiLogs.AddAsync(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
        }

        public async Task<IList<ApiLog>> GetApiLogs(string type, string username, ApiLogViewModel model)
        {
            IList<ApiLog> logs = new List<ApiLog>();

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
            if (type == UserType.AIS) logs = await dbContext.AisApiLogs.Where(a =>
                (string.IsNullOrEmpty(model.RequestId) || a.RequestId.Equals(model.RequestId)) &&
                (username.Equals("op-admin") || a.Username.Equals(username)) &&
                (string.IsNullOrEmpty(model.SearchInPath) || a.Path.Contains(model.SearchInPath)) &&
                (string.IsNullOrEmpty(model.SearchInRequest) || a.RequestBody.Contains(model.SearchInRequest))).
                Select(d =>
                    new ApiLog
                    {
                        Id = d.Id,
                        RequestId = d.RequestId,
                        Username = d.Username,
                        IpAddress = d.IpAddress,
                        Path = d.Path,
                        Method = d.Method,
                        RequestBody = d.RequestBody.Replace("\r\n", ""),
                        ResponseBody = d.ResponseBody.Replace("\r\n", ""),
                        StatusCode = d.StatusCode,
                        LogTime = d.LogTime
                    }
                ).ToListAsync();
            return logs;
        }
    }
}
