using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OP.GATEWAY.Data;
using OP.GATEWAY.Handlers;
using OP.GATEWAY.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GatewayDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("MySqlLogDb")));

builder.Configuration.AddJsonFile("op-gateway.json", optional: false, reloadOnChange: true);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("OP.Gateway.Auth", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Oman Post : Gateway API",
        Version = "v1",
        Description = "Secure API Gateway - MoL and MoH",
        Contact = new OpenApiContact
        {
            Name = "Oman Post",
            Email = "customers@omanpost.om",
            Url = new Uri("https://www.omanpost.om/")
        },
        License = new OpenApiLicense { Name = "Copyright @ omanpost.com" }
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var controllerNamespace = apiDesc.ActionDescriptor?.RouteValues["controller"];
        return controllerNamespace != null && controllerNamespace.StartsWith("Auth");
    });
});

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ILogService, LogService>();

builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<AisAuthHandler>()
    .AddDelegatingHandler<OvmcAuthHandler>();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oman Post : Gateway API");
    });
}

app.MapWhen(context => context.Request.Path.StartsWithSegments("/api/auth"), subApp =>
{
    //subApp.UseRouting();
    //subApp.UseAuthentication();
    //subApp.UseAuthorization();
    subApp.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
});

await app.UseOcelot();
await app.RunAsync();
