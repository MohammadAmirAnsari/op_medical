using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using OP.PORTAL.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OP.PORTAL.Locales
{
    public class DbStringLocalizer : IStringLocalizer
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public DbStringLocalizer(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public LocalizedString this[string name]
            => GetString(name);

        public LocalizedString this[string name, params object[] arguments]
            => GetString(name, arguments);

        private LocalizedString GetString(string key, params object[] args)
        {
            var value = string.Empty;
            var culture = CultureInfo.CurrentUICulture.Name;

            var resources = _cache.GetOrCreate(
                $"RES_{culture}",
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                    return _db.AppResources
                        .AsNoTracking()
                        .ToDictionary(x => x.ResourceKey, x => culture.Equals("ar-OM") ? x.ResourceValueArabic : x.ResourceValueEnglish);
                });

            if (resources != null && !resources.TryGetValue(key, out value))
            {
                value = key; // fallback
            }

            return new LocalizedString(
                key,
                string.Format(value, args),
                resourceNotFound: value == key
            );
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture.Name;

            return _db.AppResources
                .AsNoTracking()
                .Select(x => new LocalizedString(x.ResourceKey, culture.Equals("ar-OM") ? x.ResourceValueEnglish : x.ResourceValueArabic, false));
        }
    }
}
