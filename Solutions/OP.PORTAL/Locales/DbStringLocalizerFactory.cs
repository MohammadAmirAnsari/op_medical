using Microsoft.Extensions.Localization;

namespace OP.PORTAL.Locales
{
    public class DbStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbStringLocalizerFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            var scope = _scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<DbStringLocalizer>();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var scope = _scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<DbStringLocalizer>();
        }
    }
}
