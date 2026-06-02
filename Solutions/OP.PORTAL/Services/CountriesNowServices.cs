using System.Text.Json;

namespace OP.PORTAL.Services
{
    public class CitiesNow
    {
        public Boolean error { get; set; }
        public string msg { get; set; }

        public List<string> data { get; set; }
    }
    public class CountriesNow
    {
        public Boolean error { get; set; }
        public string msg { get; set; }

        public List<CountryNow> data { get; set; }
    }
    public class CountryNow
    {
        public string iso2 { get; set; }
        public string iso3 { get; set; }
        public string country { get; set; }

        public List<string> cities { get; set; }
    }
    public class CountriesNowServices
    {
        private readonly HttpClient _http;
        public CountriesNowServices(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("CountriesNowClient");
        }
        public async Task<List<string>> GetCountriesAsync()
        {
            var url = $"countries";
            var response = await _http.GetAsync(url);
            var resultString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                var result = JsonSerializer.Deserialize<CountriesNow>(resultString, options);
                return result?.data?.Select(c => c.country).ToList() ?? new List<string>();


        }
        public async Task<List<string>> GetCountryCitiesAsync(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.Length > 2)
            {
                var url = $"countries/cities/q?country={Uri.EscapeDataString(q)}";
                var result = await _http.GetFromJsonAsync<CitiesNow>(url);
                return result?.data?.Select(c => c).ToList() ?? new List<string>();
            }
            return new();

        }
    }

    internal record struct NewStruct(object Item1, object Item2)
    {
        public static implicit operator (object, object)(NewStruct value)
        {
            return (value.Item1, value.Item2);
        }

        public static implicit operator NewStruct((object, object) value)
        {
            return new NewStruct(value.Item1, value.Item2);
        }
    }
}
