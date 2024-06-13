namespace hh_analyzer.Application.Abstractions
{
    public interface IHHAnalyzer
    {
        Task<Dictionary<string, int>?> GetSkillsWithMentionCountFacade(
            string name, string description, CancellationToken cancellationToken);
    }
}