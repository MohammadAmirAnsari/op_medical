namespace OP.PORTAL.Services
{
    public class CountryDto
    {
        public string cca2 { get; set; }
        public CountryNameDto name { get; set; }
    }
    public class CountryNameDto
    {
        public string common { get; set; }
        public string official { get; set; }
    }
    public class RestCountriesService
    {
        private readonly HttpClient _http;
        public RestCountriesService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("RestCountriesClient");
        }
        public async Task<List<string>> GetCountriesAsync(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.Length > 2)
            {
                var url = $"name/{Uri.EscapeDataString(q)}?fields=cca2,name&lang=en";
                var result = await _http.GetFromJsonAsync<List<CountryDto>>(url);
                return result?.Select(c => c.name.common).ToList() ?? new List<string>();
            }
            return new();

        }
    }
}
