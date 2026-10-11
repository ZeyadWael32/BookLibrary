namespace BookLibrary.API.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string? Description { get; set; }x
    }
}
