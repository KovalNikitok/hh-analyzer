using hh_analyzer.Contracts;
using hh_analyzer.Application.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace hh_analyzer.Infrastructure
{
    public class TakeJobOfferApiClient : ITakeJobOfferApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly TakeJobOfferApiSettings _apiSettings;
        private readonly ILogger<TakeJobOfferApiClient> _logger;
        public TakeJobOfferApiClient(
            HttpClient httpClient,
            ILogger<TakeJobOfferApiClient> logger,
            IOptions<TakeJobOfferApiSettings> apiSettingsOptions)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiSettings = apiSettingsOptions.Value;
        }

        public async Task<List<ProfessionRequest?>?> GetProfessionsAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetProfessionsAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }
            var response = await _httpClient.GetAsync(
                $"{_apiSettings.ConnectionString}/professions",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProfessionRequest?>>(cancellationToken);
        }

        public async Task<List<ProfessionSkillWithNameRequest?>?> GetProfessionSkillWithName(
            ProfessionRequest profession,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetProfessionSkillWithName: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }

            var response = await _httpClient.GetAsync(
                $"{_apiSettings.ConnectionString}/professions-skills/{profession.Id}/with-name",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProfessionSkillWithNameRequest?>?>(cancellationToken);
        }

        public async Task<List<SkillRequest?>?> GetSkillsAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetSkillsAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }
            var response = await _httpClient.GetAsync(
                $"{_apiSettings.ConnectionString}/skills",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<SkillRequest?>>(cancellationToken);
        }

        public async Task<SkillRequest?> GetSkillByNameAsync(string name, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetSkillByNameAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }

            var response = await _httpClient.GetAsync(
                $"{_apiSettings.ConnectionString}/skills/{name}",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SkillRequest>(cancellationToken);
        }

        public async Task<Guid> SendNewSkillAsync(SkillResponse skill, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] SendSkillAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return Guid.Empty;
            }
            var jsonSkill = JsonSerializer.Serialize(skill);
            var content = new StringContent(jsonSkill, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{_apiSettings.ConnectionString}/skills",
                content,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
        }

        public async Task SendNewProfessionSkillAsync(
            ProfessionRequest profession,
            ProfessionSkillResponse ps,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] SendProfessionSkillsAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return;
            }
            var jsonProfessionSkill = JsonSerializer.Serialize(ps);
            var content = new StringContent(jsonProfessionSkill, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{_apiSettings.ConnectionString}/professions-skills/{profession.Id}",
                content,
                cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendUpdatedProfessionSkillAsync(
            ProfessionRequest profession,
            ProfessionSkillResponse ps,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] SendProfessionSkillsAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return;
            }
            var jsonProfessionSkill = JsonSerializer.Serialize(ps);
            var content = new StringContent(jsonProfessionSkill, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(
                $"{_apiSettings.ConnectionString}/professions-skills/{profession.Id}", 
                content, 
                cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
