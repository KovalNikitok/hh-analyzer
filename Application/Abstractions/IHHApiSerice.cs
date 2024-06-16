namespace hh_analyzer.Application.Abstractions
{
    public interface IHHApiSerice
    {
        Task<Dictionary<string, int>?> GetSkillsWithMentionCountFacade(
            string name, string? description, CancellationToken cancellationToken);

        void Dispose();
    }
}