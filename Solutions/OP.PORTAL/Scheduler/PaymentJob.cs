using OP.PORTAL.Services;
using Quartz;

namespace OP.PORTAL.Scheduler
{
    [DisallowConcurrentExecution]
    public class PaymentJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PaymentJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<OvmcPaymentService>();
                await paymentService.CheckStatusFromSchedulerAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
