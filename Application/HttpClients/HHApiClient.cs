using hh_analyzer.Application.HttpClients.HttpClientsSettings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace hh_analyzer.Application.HttpClients
{
    public class HHApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly HHApiSettings _apiSettings;
        public HHApiClient(HttpClient httpClient, IOptions<HHApiSettings> apiSettingsOptions)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettingsOptions.Value;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiSettings.AccessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_apiSettings.Agent);
        }
    }
}
