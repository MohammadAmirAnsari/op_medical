using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using OP.PORTAL.Locales;
using System.Globalization;

namespace OP.PORTAL.Helpers
{
    public interface ILayoutHelper
    {
        event Action? OnChange;
        CultureInfo CurrentCulture { get; }
        bool RightToLeft { get; }
        void SetLanguage(string culture);
        MarkupString SetLocaleString(string label, params object[] values);
    }

    public class LayoutHelper : ILayoutHelper
    {
        private readonly IStringLocalizer<Resource> _stringLocalizer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public event Action? OnChange;

        public CultureInfo CurrentCulture { get; private set; }
            = CultureInfo.CurrentCulture;

        public bool RightToLeft =>
            CurrentCulture.TwoLetterISOLanguageName == "ar";

        public LayoutHelper(IHttpContextAccessor httpContextAccessor, IStringLocalizer<Resource> stringLocalizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _stringLocalizer = stringLocalizer;
        }

        public void SetLanguage(string culture)
        {
            var ci = new CultureInfo(culture);

            // 🔥 THIS IS THE KEY FIX
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;

            // Optional (good practice)
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;

            CurrentCulture = ci;
            OnChange?.Invoke();
        }

        public MarkupString SetLocaleString(string label, params object[] values)
        {
            return (MarkupString) _stringLocalizer[label, values].Value;
        }

        public dynamic GetLocaleString(string label, bool returnHtmlString = false, params object[] values)
        {
            return returnHtmlString
                ? (MarkupString) _stringLocalizer[label, values].Value
                : _stringLocalizer[label, values];
        }
    }
}
