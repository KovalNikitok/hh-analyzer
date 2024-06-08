using hh_analyzer.Abstractions;
using hh_analyzer.HttpClients.HttpClientsSettings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace hh_analyzer.HttpClients
{
    public class HHApiClient : IHHApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly HHApiSettings _apiSettings;
        public HHApiClient(HttpClient httpClient, IOptions<HHApiSettings> apiSettingsOptions)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettingsOptions.Value;

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiSettings.AccessToken);
        }
    }
}
