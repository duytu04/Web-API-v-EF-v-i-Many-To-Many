using System.Collections.Generic;

namespace AuthorBookApi.Models;

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
