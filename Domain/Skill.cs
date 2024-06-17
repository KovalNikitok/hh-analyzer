namespace hh_analyzer.Domain
{
    public class Skill
    {
        public string Name { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }
    }
}
