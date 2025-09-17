namespace AuthorBookApi.Dtos;

public record AuthorCreateDto(string Name, int? BirthYear);

public record AuthorSlimDto(int AuthorId, string Name);

public record AuthorDto(int AuthorId, string Name, List<BookSlimDto> Books);
