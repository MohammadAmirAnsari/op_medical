using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OP.PORTAL.Locales
{
    [Table("portal_app_resources")]
    public class AppResource
    {
        [Key]
        public int Id { get; set; }
                
        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string ResourceKey { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string ResourceValueEnglish { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "ResxRequired", ErrorMessageResourceType = typeof(Resource))]
        public string ResourceValueArabic { get; set; } = string.Empty;
    }

    public static class LocalizerExtensions
    {
        public static MarkupString Html(
            this IStringLocalizer localizer,
            string key,
            params object[] args)
        {
            return new MarkupString(localizer[key, args].Value);
        }

        public static string Text(
            this IStringLocalizer localizer,
            string key,
            params object[] args)
        {
            return localizer[key, args].Value;
        }
    }
}
