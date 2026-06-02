using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;
using OP.PORTAL.Components;
using OP.PORTAL.Data;
using OP.PORTAL.Helpers;
using OP.PORTAL.Locales;
using OP.PORTAL.Scheduler;
using OP.PORTAL.Services;
using Quartz;
using System.Globalization;

namespace OP.PORTAL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseMySQL(connectionString ));

            builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiAuthSettings"));

            // Add MudBlazor services
            builder.Services.AddMudServices();

            builder.Services.AddHttpClient("NominatimClient", client =>
            {
                client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OP-Portal/1.0 (contact@yourdomain.com)");
            });

            builder.Services.AddHttpContextAccessor();            
            builder.Services.AddScoped<DbStringLocalizer>();
            builder.Services.AddSingleton<IStringLocalizerFactory, DbStringLocalizerFactory>();

            builder.Services.AddMemoryCache();
            builder.Services.AddLocalization();

            builder.Services.AddControllers();
            builder.Services.AddAuthorization();

            var defaultCulture = new CultureInfo("en-US");
            var supportedCultures = new[] { "en-US", "ar-OM" };

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(defaultCulture.Name)                
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
                options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            });

            builder.Services.AddSingleton<AesEncryptionHelper>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new AesEncryptionHelper(
                    config["Encryption:Key"] ?? "",
                    config["Encryption:IV"] ?? ""
                );
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddScoped<OvmcPaymentService>();
            builder.Services.AddScoped<ILayoutHelper, LayoutHelper>();
            builder.Services.AddScoped<ISponsorService, SponsorService>();
            builder.Services.AddScoped<IOvmcRequestService, OvmcRequestService>();
            builder.Services.AddScoped<IGeneralMasterService, GeneralMasterService>();

            builder.Services.AddScoped<ITokenHelper, TokenHelper>();
            builder.Services.AddScoped<ISmsHelper, SmsHelper>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<GoogleMapService>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateHelper>();
            builder.Services.AddScoped<BarcodeService>();

            var isBankStatusSchedulerEnabled = Convert.ToBoolean(builder.Configuration.GetSection("SmartPay:BankStatusScheduler").Value);
            var bankStatusSchedulerTimeInterval = Convert.ToInt16(builder.Configuration.GetSection("SmartPay:BankStatusSchedulerRunTime").Value);

            if (isBankStatusSchedulerEnabled)
            {
                builder.Services.AddQuartz(q =>
                {
                    var jobKey = new JobKey("PaymentJob");

                    q.AddJob<PaymentJob>(opts => opts.WithIdentity(jobKey));

                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("PaymentJob-trigger")
                        .WithSimpleSchedule(x => x
                            .WithInterval(TimeSpan.FromMinutes(bankStatusSchedulerTimeInterval))
                            .RepeatForever()
                        )
                    );
                });

                builder.Services.AddQuartzHostedService(q =>
                {
                    q.WaitForJobsToComplete = true;
                    q.StartDelay = TimeSpan.FromMinutes(1);
                });
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRequestLocalization(localizationOptions);
            app.UseAntiforgery();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
