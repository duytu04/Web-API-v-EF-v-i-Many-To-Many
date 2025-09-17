using AuthorBookApi.Data;
using AuthorBookApi.Dtos;
using AuthorBookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthorsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create([FromBody] AuthorCreateDto dto)
    {
        var entity = new Author { Name = dto.Name.Trim() };
        _db.Authors.Add(entity);
        await _db.SaveChangesAsync();
        var result = new AuthorDto(entity.AuthorId, entity.Name, new());
        return CreatedAtAction(nameof(GetAll), new { id = entity.AuthorId }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAll()
    {
        var items = await _db.Authors
            .Include(a => a.Books)
            .Select(a => new AuthorDto(
                a.AuthorId,
                a.Name,
                a.Books.Select(b => new BookSlimDto(b.BookId, b.Title)).ToList()
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{authorId:int}/books/{bookId:int}")]
    public async Task<IActionResult> AttachBook(int authorId, int bookId)
    {
        var author = await _db.Authors.Include(a => a.Books)
                                      .FirstOrDefaultAsync(a => a.AuthorId == authorId);
        if (author is null) return NotFound($"Author {authorId} không tồn tại.");

        var book = await _db.Books.FindAsync(bookId);
        if (book is null) return NotFound($"Book {bookId} không tồn tại.");

        if (!author.Books.Any(b => b.BookId == bookId))
            author.Books.Add(book);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Bonus: bỏ liên kết
    [HttpDelete("{authorId:int}/books/{bookId:int}")]
    public async Task<IActionResult> DetachBook(int authorId, int bookId)
    {
        var author = await _db.Authors.Include(a => a.Books)
                                      .FirstOrDefaultAsync(a => a.AuthorId == authorId);
        if (author is null) return NotFound();

        var book = author.Books.FirstOrDefault(b => b.BookId == bookId);
        if (book is null) return NotFound();

        author.Books.Remove(book);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
