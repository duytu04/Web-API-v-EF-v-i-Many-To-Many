namespace AuthorBookApi.Dtos;

public record AuthorCreateDto(string Name);
public record BookCreateDto(string Title);

public record BookSlimDto(int BookId, string Title);
public record AuthorDto(int AuthorId, string Name, List<BookSlimDto> Books);

public record AuthorSlimDto(int AuthorId, string Name);
public record BookDto(int BookId, string Title, List<AuthorSlimDto> Authors);
