namespace hh_analyzer.Domain
{
    public class Vacancies
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public int Found { get; set; }
        public int Pages { get; set; }
        public int PerPage { get; set; }
    }
}
