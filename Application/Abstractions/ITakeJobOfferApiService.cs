using hh_analyzer.Contracts;

namespace hh_analyzer.Application.Abstractions
{
    public interface ITakeJobOfferApiService
    {
        Task<List<ProfessionRequest?>?> GetProfessionsAsync(CancellationToken cancellationToken);
        Task<List<ProfessionSkillWithNameRequest?>?> GetProfessionSkillWithName(ProfessionRequest profession, CancellationToken cancellationToken);
        Task<List<SkillRequest?>?> GetSkillsAsync(CancellationToken cancellationToken);
        Task<SkillRequest?> GetSkillByNameAsync(string name, CancellationToken cancellationToken);
        Task SendNewProfessionSkillAsync(ProfessionRequest profession, ProfessionSkillResponse ps, CancellationToken cancellationToken);
        Task SendUpdatedProfessionSkillAsync(ProfessionRequest profession, ProfessionSkillResponse ps, CancellationToken cancellationToken);
        Task<Guid> SendNewSkillAsync(SkillResponse skill, CancellationToken cancellationToken);

        void Dispose();
    }
}