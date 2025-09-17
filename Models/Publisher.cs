namespace AuthorBookApi.Models;

public class Publisher
{
    public int PublisherId { get; set; }
    public string Name { get; set; } = "";
    public string? Country { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>(); // 1-N
}
