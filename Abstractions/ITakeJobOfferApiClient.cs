using hh_analyzer.Contracts;

namespace hh_analyzer.Abstractions
{
    public interface ITakeJobOfferApiClient
    {
        Task<List<ProfessionRequest?>?> GetProfessionsAsync();
        Task<List<SkillRequest?>?> GetSkillsAsync();
        Task SendProfessionSkillsAsync(ProfessionRequest profession, ProfessionSkillResponse ps);
        Task SendProfessionsSkillsAsync(ProfessionRequest profession, IEnumerable<ProfessionSkillResponse> psCollection);
        Task SendSkillAsync(SkillResponse skill);
    }
}