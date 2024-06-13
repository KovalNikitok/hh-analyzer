namespace hh_analyzer.Domain
{
    public class DetailedVacancy
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<Skill>? Skills { get; set; }
        public string? Description { get; set; }
    }
}
