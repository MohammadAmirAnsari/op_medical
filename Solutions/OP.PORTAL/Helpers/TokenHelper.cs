using Microsoft.JSInterop;

namespace OP.PORTAL.Helpers
{
    public interface ITokenHelper
    {
        Task<bool> SetTokenAsync(string token);
        Task<bool> RemoveTokenAsync();
        Task<string> GetTokenAsync();
        Task<bool> IsAuthenticated();
        Task<string?> GetUsername();
        Task<int?> GetSponsorId();
    }

    public class TokenHelper : ITokenHelper
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AesEncryptionHelper _aesEncryptionHelper;

        public TokenHelper(IJSRuntime jsRuntime, AesEncryptionHelper aesEncryptionHelper)
        {
            _jsRuntime = jsRuntime;
            _aesEncryptionHelper = aesEncryptionHelper;
        }

        public async Task<bool> SetTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            return true;
        }

        public async Task<bool> RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            return true;
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch (Exception)
            {
                return null;
            }            
        }

        public async Task<bool> IsAuthenticated()
        {
            bool isAuthenticated = false;
            var jwtToken = await GetTokenAsync();
            if (!string.IsNullOrEmpty(jwtToken))
            {
                isAuthenticated = IsTokenExpired(jwtToken);
            }
            return isAuthenticated;
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                // JWT is base64-encoded JSON: header.payload.signature
                var payload = token.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var payloadData = System.Text.Json.JsonSerializer.Deserialize<JwtPayload>(jsonBytes);

                if (payloadData == null) return false;

                // exp is seconds since Unix epoch
                var exp = DateTimeOffset.FromUnixTimeSeconds(payloadData.exp);
                return exp > DateTimeOffset.UtcNow;
            }
            catch
            {
                return false; // treat invalid token as expired
            }
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public async Task<string?> GetUsername()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return string.Empty;

            var payload = token.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='));
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var claims = System.Text.Json.JsonDocument.Parse(json).RootElement;
            return claims.TryGetProperty("Name", out var username) && !string.IsNullOrEmpty(username.GetString()) ? _aesEncryptionHelper.Decrypt(username.GetString() ?? string.Empty) : "Anonymous";
        }

        public async Task<int?> GetSponsorId()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return 0;

            var payload = token.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='));
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var claims = System.Text.Json.JsonDocument.Parse(json).RootElement;
            return claims.TryGetProperty("Id", out var userId) && !string.IsNullOrEmpty(userId.GetString()) ? Convert.ToInt32(_aesEncryptionHelper.Decrypt(userId.GetString() ?? "0")) : 0;
        }

        private class JwtPayload
        {
            public long exp { get; set; }
        }
    }
}
