using hh_analyzer.Application.Abstractions;
using hh_analyzer.Contracts;
using System.Threading;

namespace hh_analyzer.Application
{
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isInfoLogLevelEnabled = _logger.IsEnabled(LogLevel.Information);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] HHAnalyzerService running...", DateTimeOffset.Now);

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
                    var skillsWithMentionCount = await _hhApiService
                        .GetSkillsWithMentionCountFacade(
                            profession!.Name,
                            profession.Description,
                            stoppingToken);

                    if (skillsWithMentionCount is null || skillsWithMentionCount.Count == 0)
                        continue;

                    var professionSkillsWithName = await _takeJobOfferApiService
                        .GetProfessionSkillWithName(profession, stoppingToken);

                    // Update previous profession skills
                    if (professionSkillsWithName is not null || professionSkillsWithName?.Count > 0)
                    {
                        foreach (var professionSkill in professionSkillsWithName)
                        {
                            if (skillsWithMentionCount.ContainsKey(professionSkill!.Name))
                            {
                                var professionSkillResponse = new ProfessionSkillResponse(
                                    professionSkill.SkillId,
                                    professionSkill.MentionCount);

                                await _takeJobOfferApiService.SendUpdatedProfessionSkillAsync(
                                    profession,
                                    professionSkillResponse,
                                    stoppingToken);

                                // Remove processed profession skills
                                skillsWithMentionCount.Remove(professionSkill!.Name);
                            }
                        }
                    }

                    // Remove skills with mentions lesser then 'requiredMentionsCount'
                    int requiredMentionsCount = 2;
                    int skillsWithMentions = GetSkillsWithMentionsGreaterThen(
                        requiredMentionsCount,
                        skillsWithMentionCount);

                    // Break, if no one skill have at least 'requiredMentionsCount' mentions
                    if (skillsWithMentions < 1)
                        break;

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
                            profession,
                            new ProfessionSkillResponse(skillId, skill.Value),
                            stoppingToken);

                        // Load balancing with waiting 50ms every sended skill for a specific profession
                        await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                    }

                    // Load balancing with waiting 3s every processed profession
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }

                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] HHAnalyzerService ended...", DateTimeOffset.Now);
                // 1 day delay before next HHBackgroundService service start
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
        
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

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
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
