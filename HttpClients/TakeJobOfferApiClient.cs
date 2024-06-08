using hh_analyzer.Contracts;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using hh_analyzer.HttpClients.HttpClientsSettings;
using hh_analyzer.Abstractions;

namespace hh_analyzer.HttpClients
{
    public class TakeJobOfferApiClient : ITakeJobOfferApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly TakeJobOfferApiSettings _apiSettings;
        public TakeJobOfferApiClient(HttpClient httpClient, IOptions<TakeJobOfferApiSettings> apiSettingsOptions)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettingsOptions.Value;
        }

        public async Task<List<ProfessionRequest?>?> GetProfessionsAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiSettings.ConnectionString}/professions");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProfessionRequest?>>();
        }

        public async Task<List<SkillRequest?>?> GetSkillsAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiSettings.ConnectionString}/skills");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<SkillRequest?>>();
        }

        public async Task SendSkillAsync(SkillResponse skill)
        {
            var jsonSkill = JsonSerializer.Serialize(skill);
            var content = new StringContent(jsonSkill, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiSettings.ConnectionString}/skills", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendProfessionSkillsAsync(ProfessionRequest profession, ProfessionSkillResponse ps)
        {
            var jsonProfessionSkill = JsonSerializer.Serialize(ps);
            var content = new StringContent(jsonProfessionSkill, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{_apiSettings.ConnectionString}/professions-skills/{profession.Id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendProfessionsSkillsAsync(ProfessionRequest profession, IEnumerable<ProfessionSkillResponse> psCollection)
        {
            var jsonProfessionSkills = JsonSerializer.Serialize(psCollection);
            var content = new StringContent(jsonProfessionSkills, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{_apiSettings.ConnectionString}/professions-skills/{profession.Id}", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
