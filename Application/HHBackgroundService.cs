using hh_analyzer.Application.Abstractions;
using hh_analyzer.Contracts;

namespace hh_analyzer.Application
{
    public class HHBackgroundService : BackgroundService
    {
        private readonly ILogger<HHBackgroundService> _logger;
        private readonly ITakeJobOfferApiClient _takeJobOfferApiClient;
        private readonly IHHAnalyzer _hhAnalyzer;

        public HHBackgroundService(ILogger<HHBackgroundService> logger,
            ITakeJobOfferApiClient takeJobOfferApiClient,
            IHHAnalyzer hhAnalyzer)
        {
            _logger = logger;
            _takeJobOfferApiClient = takeJobOfferApiClient;
            _hhAnalyzer = hhAnalyzer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isInfoLogLevelEnabled = _logger.IsEnabled(LogLevel.Information);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] HHAnalyzerService running...", DateTimeOffset.Now);

                // Get professions from takejoboffer api
                var professions = await _takeJobOfferApiClient.GetProfessionsAsync(stoppingToken);

                // If professions is null = wait 3 hours and repeat
                if (professions is null || professions.Count == 0)
                {
                    await Task.Delay(new TimeSpan(3, 0, 0), stoppingToken);
                    continue;
                }

                foreach (var profession in professions)
                {
                    var skillsWithMentionCount = await _hhAnalyzer
                        .GetSkillsWithMentionCountFacade(
                            profession!.Name,
                            profession.Description,
                            stoppingToken);

                    if (skillsWithMentionCount is null || skillsWithMentionCount.Count == 0)
                        continue;

                    var professionSkillsWithName = await _takeJobOfferApiClient
                        .GetProfessionSkillWithName(profession, stoppingToken);

                    // Update previous profession skills
                    if (professionSkillsWithName is not null || professionSkillsWithName!.Count > 0)
                    {
                        foreach (var professionSkill in professionSkillsWithName)
                        {
                            if (skillsWithMentionCount.ContainsKey(professionSkill!.Name))
                            {
                                var professionSkillResponse = new ProfessionSkillResponse(
                                    professionSkill.SkillId,
                                    professionSkill.MentionCount);

                                await _takeJobOfferApiClient.SendUpdatedProfessionSkillAsync(
                                    profession,
                                    professionSkillResponse,
                                    stoppingToken);

                                // Remove processed profession skills
                                skillsWithMentionCount.Remove(professionSkill!.Name);
                            }
                        }
                    }

                    // Added new skills (if needed) and profession skills
                    foreach (var skill in skillsWithMentionCount)
                    {
                        var skillRequest = await _takeJobOfferApiClient.GetSkillByNameAsync(
                            skill.Key,
                            stoppingToken);

                        Guid skillId = skillRequest?.Id ?? Guid.Empty;
                        if (skillRequest is null)
                            skillId = await _takeJobOfferApiClient.SendNewSkillAsync(
                                new SkillResponse(skill.Key),
                                stoppingToken);

                        if (skillId == Guid.Empty)
                            continue;

                        await _takeJobOfferApiClient.SendNewProfessionSkillAsync(
                            profession,
                            new ProfessionSkillResponse(skillId, skill.Value),
                            stoppingToken);
                    }
                }

                // Setting a pause with time offset on 1 day
                await Task.Delay(new TimeSpan(1, 0, 0, 0), stoppingToken);
            }
        }
    }
}
