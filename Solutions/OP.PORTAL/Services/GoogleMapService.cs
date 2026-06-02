namespace OP.PORTAL.Services
{
    public class NominatimDto
    {
        public string addresstype { get; set; }
        public string name { get; set; }       
        public NominatimAddressDto address { get; set; }
    }
    public class NominatimAddressDto
    {
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
    }

    public class GoogleMapService
    {
        private readonly HttpClient _http;

        public GoogleMapService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("NominatimClient");
        }

        public async Task<List<string>> SearchPlacesAsync(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.Length > 2)
            {
                var url = $"search?q={Uri.EscapeDataString(q)}&format=json&addressdetails=1&accept-language=en";

                var result = await _http.GetFromJsonAsync<List<NominatimDto>>(url);

                return result?
                    .Where(l => l.addresstype.Equals("city"))
                    .Select(x => x.address.city + ", " + x.address.country)
                    .Distinct()
                    .Take(10)
                    .ToList() ?? new();
            }
            return new();
        }
    }
}
