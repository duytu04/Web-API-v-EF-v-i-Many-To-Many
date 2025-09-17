using System.Collections.Generic;

namespace AuthorBookApi.Models;

public class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public int PublishedYear { get; set; }

    // 1-N: mỗi Book có một Publisher
    public int PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    // N-N: Book <-> Authors
    public ICollection<Author> Authors { get; set; } = new List<Author>();
}
