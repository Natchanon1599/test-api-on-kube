using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using TestKubeApi.Interfaces;
using TestKubeLibrary.Models.Services;
namespace TestKubeApi.Services
{
    public class TestKubeService : ITestKubeService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public TestKubeService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private string ApiServiceUrl()
        {
            return (_configuration != null) ? _configuration.GetValue<string>("ServiceAPI:ApiUrl") ?? string.Empty : string.Empty;
        }
        private readonly string apiController = "todos";
        public async Task<ActionResult<JsonTest>> GetData()
        {
            string url = $"{ApiServiceUrl()}/{apiController}/1";
            JsonTest? response = await _httpClient.GetFromJsonAsync<JsonTest>(url) ?? new JsonTest();
            return response;
        }
    }

}