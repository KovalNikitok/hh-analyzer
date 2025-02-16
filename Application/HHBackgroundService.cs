using hh_analyzer.Application.Abstractions;
using hh_analyzer.Contracts;

namespace hh_analyzer.Application
{
    /// <summary>
    /// Служба для ежедневного опроса HeadHunter (HH) API
    /// </summary>
    public class HHBackgroundService : BackgroundService, IDisposable
    {
        private bool _disposed;

        private readonly ILogger<HHBackgroundService> _logger;
        private readonly ITakeJobOfferApiService _takeJobOfferApiService;
        private readonly IHHApiSerice _hhApiService;

        public HHBackgroundService(
            IHHApiSerice hhApiService,
            ITakeJobOfferApiService takeJobOfferApiService,
            ILogger<HHBackgroundService> logger)
        {
            _logger = logger;
            _takeJobOfferApiService = takeJobOfferApiService;
            _hhApiService = hhApiService;
        }

        /// <summary>
        /// Метод для исполнения запроса на получение данных по вакансиям с HeadHunter (HH)
        /// </summary>
        /// <param name="stoppingToken">Токен для прерывания запроса</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isInfoLogLevelEnabled = _logger.IsEnabled(LogLevel.Information);

            while (!stoppingToken.IsCancellationRequested)
            {
                var serviceStartTime = DateTime.Now.TimeOfDay;
                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] HHAnalyzerService running", DateTimeOffset.Now);

                // Get professions from takejoboffer api
                var professions = await _takeJobOfferApiService.GetProfessionsAsync(stoppingToken);

                // If professions is null than wait 3 hours and repeat
                if (professions is null || professions.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
                    continue;
                }

                foreach (var profession in professions)
                {
                    Dictionary<string, int>? skillsWithMentionCount = null;
                    try
                    {
                        skillsWithMentionCount = await _hhApiService
                        .GetSkillsWithMentionCountFacade(
                            profession!.Name,
                            profession.Description,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            "[{time}] [{message}]",
                            DateTimeOffset.Now,
                            ex.Message);
                    }

                    if (skillsWithMentionCount is null || skillsWithMentionCount.Count == 0)
                        continue;

                    var professionSkillsWithName = await _takeJobOfferApiService
                        .GetProfessionSkillWithName(profession!, stoppingToken);

                    int requiredMentionsCount = 4;

                    // Update existing profession skills
                    if (professionSkillsWithName is not null || professionSkillsWithName?.Count > 0)
                    {
                        foreach (var professionSkill in professionSkillsWithName)
                        {
                            if (!skillsWithMentionCount.ContainsKey(professionSkill!.Name))
                            {
                                continue;
                            }
                            // Remove processed profession skills
                            skillsWithMentionCount.Remove(professionSkill!.Name);

                            if (professionSkill.MentionCount < requiredMentionsCount)
                            {
                                var professionSkillRequest = new ProfessionSkillRequest(professionSkill!.SkillId);

                                await _takeJobOfferApiService.RemoveProfessionSkillAsync(
                                    profession!,
                                    professionSkillRequest,
                                    stoppingToken);

                                continue;
                            }

                            var professionSkillResponse = new ProfessionSkillResponse(
                                professionSkill.SkillId,
                                professionSkill.MentionCount);

                            await _takeJobOfferApiService.SendUpdatedProfessionSkillAsync(
                                profession!,
                                professionSkillResponse,
                                stoppingToken);
                        }
                    }

                    // Remove skills with mentions lesser then 'requiredMentionsCount'
                    int skillsWithMentions = GetSkillsWithMentionsGreaterThen(
                        requiredMentionsCount,
                        skillsWithMentionCount);

                    // Continue to next profession, if no one skill have at least 'requiredMentionsCount' mentions
                    if (skillsWithMentions < 1)
                        continue;

                    // Added new skills (if needed) and profession skills
                    foreach (var skill in skillsWithMentionCount)
                    {
                        var skillRequest = await GetSkillByNameAsync(skill.Key, stoppingToken);

                        Guid skillId = skillRequest?.Id ?? Guid.Empty;

                        if (skillRequest is null)
                            skillId = await _takeJobOfferApiService.SendNewSkillAsync(
                                new SkillResponse(skill.Key),
                                stoppingToken);

                        if (skillId == Guid.Empty)
                            continue;

                        await _takeJobOfferApiService.SendNewProfessionSkillAsync(
                            profession!,
                            new ProfessionSkillResponse(skillId, skill.Value),
                            stoppingToken);

                        // Load balancing with waiting 100ms every sended skill for a specific profession
                        await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                    }

                    // Load balancing with waiting 3m every processed profession
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }

                TimeSpan serviceEndTime = DateTime.Now.TimeOfDay;
                TimeSpan duration = serviceEndTime - serviceStartTime;

                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] HHAnalyzerService ended, working time is {duration}...", DateTimeOffset.Now, duration);

                // 1 day delay before next HHBackgroundService service start
                await Task.Delay(TimeSpan.FromDays(1) - duration, stoppingToken);
            }
        }
        
        /// <summary>
        /// Метод для получения навыка по его названию
        /// </summary>
        /// <param name="skillName">Название навыка</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns></returns>
        private async Task<SkillRequest?> GetSkillByNameAsync(string skillName, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        "[{time}] Service-GetSkillByNameAsync: CancellationTokenRequested",
                        DateTimeOffset.Now);
                return null;
            }

            var skillRequest = await _takeJobOfferApiService.GetSkillByNameAsync(
                            skillName,
                            cancellationToken);

            return skillRequest;
        }

        /// <summary>
        /// Метод для отбора ннавыков с количеством упоминаний не меньшее, чем заданное значение (mentionsCount)
        /// </summary>
        /// <param name="mentionsCount">Количество упоминаний навыка по профессии</param>
        /// <param name="skillsWithMentionCount">Словарь с навыками и количеством их упоминаний</param>
        /// <returns>int: количество навыков, соответствующее желаемому количеству упоминаний (mentionsCount)</returns>
        private static int GetSkillsWithMentionsGreaterThen(
            int mentionsCount, 
            Dictionary<string, int> skillsWithMentionCount)
        {
            foreach (var skill in skillsWithMentionCount)
            {
                if(skill.Value <= mentionsCount)
                    skillsWithMentionCount.Remove(skill.Key);
            }

            return skillsWithMentionCount.Count;
        }

        /// <summary>
        /// Реализация интерфейса Dispose для освобождения ресурсов
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Метод для освобождения ресурсов по паттерну Dispose
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (_disposed)
                return;

            if (isDisposing)
            {
                _takeJobOfferApiService.Dispose();
                _hhApiService.Dispose();
                base.Dispose();
            }

            _disposed = true;
        }
    }
}
