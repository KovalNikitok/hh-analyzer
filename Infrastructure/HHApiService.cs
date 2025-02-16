using hh_analyzer.Application.Abstractions;
using hh_analyzer.Domain;
using hh_analyzer.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace hh_analyzer.Infrastructure
{
    public class HHApiService : IHHApiSerice, IDisposable
    {
        private bool _disposed;
        private readonly JsonSerializerOptions _serializerOptions;


        private readonly HttpClient _httpClient;
        private readonly ILogger<HHApiService> _logger;
        private readonly HHApiSettings _settings;
        public HHApiService(
            IHttpClientFactory httpClientFactory,
            IOptions<HHApiSettings> apiSettingsOptions,
            ILogger<HHApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _settings = apiSettingsOptions.Value;
            _logger = logger;

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new Int32Converter(),
                    new DateTimeConverter(),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                },
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            Initialize();
        }

        private void Initialize()
        {
            _httpClient.BaseAddress = new Uri(_settings.ConnectionString);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_settings.Agent);
        }


        public async Task<Dictionary<string, int>?> GetSkillsWithMentionCountFacade(
            string name, string? description, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetVacancies: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            bool isErrorLogLevelEnabled = _logger.IsEnabled(LogLevel.Error);

            var vacancies = await GetVacancies(name, description, cancellationToken);
            if (vacancies is null)
            {
                if (isErrorLogLevelEnabled)
                    _logger.LogError(
                        "[{time}] GetSkillsWithMentionCountFacade: 'GetVacancies' (vacancies) is null",
                        DateTimeOffset.Now);
                throw new NullReferenceException("'GetVacancies' is null");
            }

            var vacanciesIds = await GetVacanciesIdsByProfession(
                vacancies, name, description, cancellationToken);
            if (vacanciesIds is null)
            {
                if (isErrorLogLevelEnabled)
                    _logger.LogError(
                        "[{time}] GetSkillsWithMentionCountFacade: 'GetVacanciesIdsByProfession' (vacanciesIds) is null",
                        DateTimeOffset.Now);
                throw new NullReferenceException("'GetVacanciesIdsByProfession' is null");
            }

            var skillsList = await GetSkillsFromVacancies(vacanciesIds, cancellationToken);
            if (skillsList is null)
            {
                if (isErrorLogLevelEnabled)
                    _logger.LogError(
                        "[{time}] GetSkillsWithMentionCountFacade: 'GetSkillsFromVacancies' (skillsList) is null",
                        DateTimeOffset.Now);
                throw new NullReferenceException("'GetSkillsFromVacancies' is null");
            }

            if (skillsList?.Count == 0)
                return null;

            var skillsWithMentions = GetSkillsMentionCount(skillsList!);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                foreach (var skill in skillsWithMentions)
                {
                    _logger.LogInformation(
                        "[{time}] GetSkillsMentionCount: skill: {skill} - mentions: {mentionCount}",
                        DateTimeOffset.Now, skill.Key, skill.Value);
                }
            }

            return skillsWithMentions;
        }


        private async Task<Vacancies?> GetVacancies(
            string name, string? description, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetVacancies: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }

            var response = await _httpClient.GetAsync(
                Link(name, description),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        message: $"[{DateTimeOffset.Now}] GetVacancies: " +
                        $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                return null;
            }

            Vacancies? vacancies = await response.Content.ReadFromJsonAsync<Vacancies?>(
                _serializerOptions,
                cancellationToken);

            if (vacancies is null || vacancies?.Items is null || vacancies?.Items.Count == 0)
                return null;

            return vacancies;
        }
        private async Task<List<int>?> GetVacanciesIdsByProfession(
            Vacancies vacancies, string name, string? description,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetVacancies: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }

            var vacanciesIds = new List<int>();
            var pages = vacancies.Pages;

            // Free version of access to hh api has 2k vacancy request limit
            if (pages > 5)
                pages = 5;

            for (int i = 0; i < pages; i++)
            {
                var response = await _httpClient.GetAsync(
                    $"{Link(name, description)}&page={i + 1}",
                    cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning(
                            message: $"[{DateTimeOffset.Now}] GetVacanciesIdsByProfession: " +
                            $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                    return null;
                }

                var currVacancies = await response.Content.ReadFromJsonAsync<Vacancies>(
                    _serializerOptions,
                    cancellationToken);

                var itemsList = currVacancies?.Items;

                if (itemsList is null || itemsList.Count == 0)
                    return vacanciesIds;

                foreach (var item in itemsList)
                    vacanciesIds.Add(item.Id);
            }
            return vacanciesIds;
        }

        private async Task<List<Skill>?> GetSkillsFromVacancies(
            List<int> vacanciesIds, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] GetVacancies: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }

            bool isInfoLogLevelEnabled = _logger.IsEnabled(LogLevel.Information);
            var skills = new List<Skill>();
            foreach (var vacancyId in vacanciesIds)
            {
                var response = await _httpClient.GetAsync(
                    $"{_httpClient.BaseAddress}vacancies/{vacancyId}",
                    cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning(
                            message: $"[{DateTimeOffset.Now}] GetSkillsFromVacancies: " +
                            $"{await response.Content.ReadAsStringAsync(cancellationToken)}");
                    return null;
                }

                var detailedVacancy = await response.Content.ReadFromJsonAsync<DetailedVacancy>(
                    _serializerOptions,
                    cancellationToken);

                if (response is null || detailedVacancy?.Skills is null)
                    continue;

                if (detailedVacancy.Skills.Count > 0)
                {
                    skills.AddRange(detailedVacancy.Skills);
                    _logger.LogInformation(
                        "[{time}] GetSkillsFromVacancies: {skills}",
                        DateTimeOffset.Now, string.Join(", ", detailedVacancy?.Skills!));
                }
            }
            return skills;
        }

        private static Dictionary<string, int> GetSkillsMentionCount(List<Skill> skillsName)
        {
            var skillsMentionCount = new Dictionary<string, int>();
            foreach (var skill in skillsName)
            {
                if (!skillsMentionCount.ContainsKey(skill.Name))
                    skillsMentionCount[skill.Name] = 0;
                skillsMentionCount[skill.Name]++;
            }
            return skillsMentionCount;
        }

        private string Link(string name, string? description)
        {
            var link = new StringBuilder();
            var encodedName = WebUtility.UrlEncode($"'{name}'");
            link.Append($"{_httpClient.BaseAddress}vacancies?text=Name%3A%28{encodedName}%29+");

            /* Uncomment, if needed (!)
            if (!string.IsNullOrEmpty(description))
            {
                var encodedDescription = WebUtility.UrlEncode($"'{description}'");
                link.Append($"and+DESCRIPTION%3A%28\"{encodedDescription}\"%29+");
            }
            */
            link.Append("NOT+%D0%BC%D0%B5%D0%BD%D1%82%D0%BE%D1%80+NOT+Senior+not+%D0%9F%D1%80%D0%B5%D0%BF%D0%BE%D0%B4%D0%B0%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8C+NOT+TechLead+NOT+%D1%82%D0%B5%D1%85%D0%BB%D0%B8%D0%B4&per_page=100");

            return link.ToString();
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
