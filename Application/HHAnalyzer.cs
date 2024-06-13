using hh_analyzer.Application.Abstractions;
using hh_analyzer.Application.HttpClients.HttpClientsSettings;
using hh_analyzer.Domain;
using System.Net.Http.Json;

namespace hh_analyzer.Application
{
    public class HHAnalyzer : IHHAnalyzer
    {
        private readonly ILogger<HHBackgroundService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly HHApiSettings _settings;

        public HHAnalyzer(ILogger<HHBackgroundService> logger,
            IHttpClientFactory httpClientFactory, HHApiSettings settings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            _httpClient = _httpClientFactory.CreateClient();
            _settings = settings;
        }

        private string Link(string name, string description) =>
            $"{_settings.ConnectionString}/vacancies?text=Name%3A%28\"{name}\"%29+and+DESCRIPTION%3A%28\"{description}\"%29+NOT+%D0%BC%D0%B5%D0%BD%D1%82%D0%BE%D1%80+NOT+Senior+not+%D0%9F%D1%80%D0%B5%D0%BF%D0%BE%D0%B4%D0%B0%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8C+NOT+TechLead+NOT+%D1%82%D0%B5%D1%85%D0%BB%D0%B8%D0%B4&per_page=100";

        public async Task<Dictionary<string, int>?> GetSkillsWithMentionCountFacade(
            string name, string description, CancellationToken cancellationToken)
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
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("description");
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

            if (skillsList.Count == 0)
                return null;

            var skillsWithMentions = GetSkillsMentionCount(skillsList);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                foreach (var skill in skillsWithMentions)
                {
                    _logger.LogInformation(
                        "[{time}] GetSkillsMentionCount: skill: {skill} - metions: {mentionCount}",
                        DateTimeOffset.Now, skill.Key, skill.Value);
                }
            }

            return skillsWithMentions;
        }


        private async Task<Vacancies?> GetVacancies(
            string name, string description, CancellationToken cancellationToken)
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
            response.EnsureSuccessStatusCode();

            Vacancies? vacancies = await response.Content.ReadFromJsonAsync<Vacancies?>(
                cancellationToken);

            if (vacancies is null || vacancies?.Items is null || vacancies?.Items.Count == 0)
                return null;

            return vacancies;
        }
        private async Task<List<int>?> GetVacanciesIdsByProfession(
            Vacancies vacancies, string name, string description,
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
            for (int i = 0; i < pages; i++)
            {
                var response = await _httpClient.GetAsync(
                    $"{Link(name, description)}&page={++i}",
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                var currVacancies = await response.Content.ReadFromJsonAsync<Vacancies>(
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
                    $"{_settings}/vacancies /{vacancyId}",
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                var detailedVacancy = await response.Content.ReadFromJsonAsync<DetailedVacancy>(
                    cancellationToken);

                if (response is null || detailedVacancy?.Skills is null)
                    continue;

                skills.AddRange(detailedVacancy.Skills);

                if (isInfoLogLevelEnabled)
                    _logger.LogInformation(
                        "[{time}] GetSkillsFromVacancies: {skills}",
                        DateTimeOffset.Now, detailedVacancy?.Skills);
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
    }
}
