namespace hh_analyzer.Domain
{
    public class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Salary? Salary { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
