using System.Net.Http.Json;
using System.Text.Json;
using TestKubeApi.Interfaces;
using TestKubeLibrary.Models.Services;
namespace TestKubeApi.Services
{
    // Calls the external store API (ServiceAPI:StoreApiUrl, e.g. https://fakestoreapi.com).
    public class StoreService : IStoreService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public StoreService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private string ApiServiceUrl()
        {
            return (_configuration != null) ? _configuration.GetValue<string>("ServiceAPI:StoreApiUrl") ?? string.Empty : string.Empty;
        }
        private readonly string apiController = "products";

        public async Task<List<StoreAPIRs>> GetProducts(CancellationToken ct = default)
        {
            string url = $"{ApiServiceUrl()}/{apiController}";
            List<StoreAPIRs>? response = await _httpClient.GetFromJsonAsync<List<StoreAPIRs>>(url, ct) ?? new List<StoreAPIRs>();
            return response;
        }

        // Returns null when the product does not exist (business logic turns that into NotFoundException)
        public async Task<StoreAPIRs?> GetProductById(int id, CancellationToken ct = default)
        {
            string url = $"{ApiServiceUrl()}/{apiController}/{id}";
            using var httpResponse = await _httpClient.GetAsync(url, ct);
            httpResponse.EnsureSuccessStatusCode();

            // fakestoreapi answers 200 with an empty body for an unknown id
            string body = await httpResponse.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            return JsonSerializer.Deserialize<StoreAPIRs>(body);
        }
    }

}
