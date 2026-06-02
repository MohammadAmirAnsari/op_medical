using Microsoft.EntityFrameworkCore;
using OP.GATEWAY.Data;
using OP.GATEWAY.Helpers;
using OP.GATEWAY.Models;
using System.Text.Json;
using System.Xml.Serialization;

namespace OP.GATEWAY.Services
{
    public interface IAuthService
    {
        Task<AuthUserViewModel?> ValidateGatewayUserAsync(LoginViewModel request);

        Task<string?> ValidateDownstreamUserAsync(string userType);

        string? GetOvmcApiAuthKey();
    }

    public class AuthService : IAuthService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public AuthService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        private async Task<ApiUser?> GetUserByCredentials(string userType, string username, string password)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
            var passwordHash = CryptoHelper.ComputeSha256Hash(password);

            return await dbContext.ApiUsers.FirstOrDefaultAsync(u => u.UserType.Equals(userType) && u.IsActive &&
                u.Username.Equals(username) && u.PasswordHash.Equals(passwordHash));
        }

        public async Task<AuthUserViewModel?> ValidateGatewayUserAsync(LoginViewModel request)
        {
            try
            {
                ApiUser? user = await this.GetUserByCredentials(UserType.GATEWAY, request.Username, request.Password);
                return user != null ? new AuthUserViewModel { Username = user.Username, UserType = user.UserType } : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string?> ValidateDownstreamUserAsync(string userType)
        {
            try
            {
                string? apiUrl = _configuration.GetSection($"DownstreamCredentials:{userType}:AuthUrl").Value;
                using (HttpClient client = new())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl ?? string.Empty);
                    if (response.IsSuccessStatusCode)
                    {
                        string xmlString = await response.Content.ReadAsStringAsync();
                        var serializer = new XmlSerializer(typeof(AuthResponse));
                        using var reader = new StringReader(xmlString);
                        if (reader != null)
                        {
                            AuthResponse? result = serializer.Deserialize(reader) as AuthResponse;
                            return result?.Message;
                        }
                    }
                    else
                        AppLogHelper.WriteLog("AIS API Token response : " + JsonSerializer.Serialize(response));
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                AppLogHelper.WriteLog("AIS API Token Error : " + ex.Message);
                return string.Empty;
            }
        }

        public string? GetOvmcApiAuthKey()
        {
            try
            {
                return _configuration.GetSection($"DownstreamCredentials:OVMC:AuthKey").Value;
            }
            catch (Exception ex)
            {
                AppLogHelper.WriteLog("OVMC API Token Error : " + ex.Message);
                return string.Empty;
            }
        }
    }
}
