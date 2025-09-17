



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
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

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
            .AsNoTracking()
            // Không cần Include vì đang projection ra DTO
            .Select(a => new AuthorDto(
                a.AuthorId,
                a.Name,
                a.Books
                    .Select(b => new BookSlimDto(b.BookId, b.Title))
                    .ToList()
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{authorId:int}/books/{bookId:int}")]
    public async Task<IActionResult> AttachBook(int authorId, int bookId)
    {
        var author = await _db.Authors
                              .Include(a => a.Books)
                              .FirstOrDefaultAsync(a => a.AuthorId == authorId);
        if (author is null) return NotFound($"Author {authorId} không tồn tại.");

        var book = await _db.Books.FindAsync(bookId);
        if (book is null) return NotFound($"Book {bookId} không tồn tại.");

        if (!author.Books.Any(b => b.BookId == bookId))
            author.Books.Add(book);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{authorId:int}/books/{bookId:int}")]
    public async Task<IActionResult> DetachBook(int authorId, int bookId)
    {
        var author = await _db.Authors
                              .Include(a => a.Books)
                              .FirstOrDefaultAsync(a => a.AuthorId == authorId);
        if (author is null) return NotFound();

        var book = author.Books.FirstOrDefault(b => b.BookId == bookId);
        if (book is null) return NotFound();

        author.Books.Remove(book);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ======================
    // LINQ endpoints
    // ======================

    // GET: api/authors/{authorId}/books
    [HttpGet("{authorId:int}/books")]
    public async Task<IActionResult> BooksOfAuthor(int authorId)
    {
        // (Tuỳ chọn) Bật block sau nếu muốn 404 khi authorId không tồn tại
        // var exists = await _db.Authors.AsNoTracking().AnyAsync(a => a.AuthorId == authorId);
        // if (!exists) return NotFound();

        var q = await _db.Books.AsNoTracking()
                 .Where(b => b.Authors.Any(a => a.AuthorId == authorId))
                 .Select(b => new { b.BookId, b.Title, b.PublishedYear })
                 .ToListAsync();
        return Ok(q);
    }

    // GET: api/authors/prolific?min=2
    [HttpGet("prolific")]
    public async Task<IActionResult> Prolific([FromQuery] int min = 2)
    {
        var q = await _db.Authors.AsNoTracking()
                 .Select(a => new { a.AuthorId, a.Name, Count = a.Books.Count })
                 .Where(x => x.Count > min)
                 .OrderByDescending(x => x.Count)
                 .ToListAsync();
        return Ok(q);
    }
}
