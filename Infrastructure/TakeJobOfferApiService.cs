using hh_analyzer.Contracts;
using hh_analyzer.Application.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using hh_analyzer.Infrastructure.Settings;
using System.Net.Http.Headers;
using System.Runtime;

namespace hh_analyzer.Infrastructure
{
    public class TakeJobOfferApiService : ITakeJobOfferApiService, IDisposable
    {
        private bool _disposed;

        private readonly HttpClient _httpClient;
        private readonly TakeJobOfferApiSettings _apiSettings;
        private readonly ILogger<TakeJobOfferApiService> _logger;
        public TakeJobOfferApiService(
            IHttpClientFactory httpClientFactory,
            ILogger<TakeJobOfferApiService> logger,
            IOptions<TakeJobOfferApiSettings> apiSettingsOptions)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiSettings = apiSettingsOptions.Value;

            Initialize();
        }

        private void Initialize()
        {
            _httpClient.BaseAddress = new Uri(_apiSettings.ConnectionString);
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
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
                $"{_httpClient.BaseAddress}/professions",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] GetProfessionsAsync: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return null;
            }

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
                $"{_httpClient.BaseAddress}/professions-skills/{profession.Id}/with-name",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] GetProfessionSkillWithName: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return null;
            }

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
                $"{_httpClient.BaseAddress}/skills",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] GetSkillsAsync: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return null;
            }

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
                $"{_httpClient.BaseAddress}/skills/{name}",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] GetSkillByNameAsync: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return null;
            }

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
                $"{_httpClient.BaseAddress}/skills",
                content,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] SendNewSkillAsync: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return Guid.Empty;
            }

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
                $"{_httpClient.BaseAddress}/professions-skills/{profession.Id}",
                content,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] SendNewSkillAsync: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return;
            }
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
                $"{_httpClient.BaseAddress}/professions-skills/{profession.Id}", 
                content, 
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] SendNewSkillAsync: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool isDisposing)
        {
            if (_disposed)
                return;

            if (isDisposing)
                _httpClient.Dispose();

            _disposed = true;
        }
    }
}
