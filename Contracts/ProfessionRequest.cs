namespace hh_analyzer.Contracts
{
    public class ProfessionRequest
    {
        public Guid Id { get; }
        public string Name { get; } = string.Empty;
        public string? Description { get; }
    }
}
