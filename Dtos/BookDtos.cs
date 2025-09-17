namespace AuthorBookApi.Dtos;

public record BookCreateDto(string Title, int PublishedYear, int PublisherId);

public record BookSlimDto(int BookId, string Title);

public record BookDto(int BookId, string Title, List<AuthorSlimDto> Authors);

public record BookViewDto(
    int BookId,
    string Title,
    int PublishedYear,
    string Publisher,
    List<string> Authors
);
