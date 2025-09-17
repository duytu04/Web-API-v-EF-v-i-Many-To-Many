using AuthorBookApi.Data;
using AuthorBookApi.Dtos;
using AuthorBookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;
    public BooksController(AppDbContext db) => _db = db;

    // POST: api/books
    // Tạo Book mới (kèm PublishedYear, PublisherId)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookCreateDto dto)
    {
        // Kiểm tra Publisher tồn tại
        var hasPub = await _db.Publishers
                              .AnyAsync(p => p.PublisherId == dto.PublisherId);
        if (!hasPub) return BadRequest("PublisherId không tồn tại");

        var entity = new Book
        {
            Title = dto.Title?.Trim() ?? "",
            PublishedYear = dto.PublishedYear,
            PublisherId = dto.PublisherId
        };

        _db.Books.Add(entity);
        await _db.SaveChangesAsync();

        // Trả về 201 kèm route Get(id)
        return CreatedAtAction(nameof(Get), new { id = entity.BookId }, new
        {
            entity.BookId,
            entity.Title,
            entity.PublishedYear,
            entity.PublisherId
        });
    }

    // GET: api/books/{id}
    // Trả DTO gồm Publisher + Authors (projection, không Include)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _db.Books.AsNoTracking()
            .Where(b => b.BookId == id)
            .Select(b => new BookViewDto(
                b.BookId,
                b.Title,
                b.PublishedYear,
                b.Publisher!.Name,
                b.Authors.Select(a => a.Name).ToList()
            ))
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    // GET: api/books
    // Danh sách (projection, không Include) để tránh truy vấn phức tạp
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var q = await _db.Books.AsNoTracking()
                 .OrderBy(b => b.Title)
                 .Select(b => new BookViewDto(
                     b.BookId,
                     b.Title,
                     b.PublishedYear,
                     b.Publisher!.Name,
                     b.Authors.Select(a => a.Name).ToList()
                 ))
                 .ToListAsync();
        return Ok(q);
    }

    // POST: api/books/{bookId}/authors/{authorId}
    // Gắn Author vào Book (N-N)
    [HttpPost("{bookId:int}/authors/{authorId:int}")]
    public async Task<IActionResult> AttachAuthor(int bookId, int authorId)
    {
        var book = await _db.Books
                            .Include(b => b.Authors)
                            .FirstOrDefaultAsync(b => b.BookId == bookId);
        var author = await _db.Authors.FindAsync(authorId);

        if (book is null || author is null) return NotFound();

        if (!book.Authors.Any(x => x.AuthorId == authorId))
        {
            book.Authors.Add(author);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    // PUT: api/books/{bookId}/publisher/{publisherId}
    // Gắn/đổi Publisher cho Book (1-N)
    [HttpPut("{bookId:int}/publisher/{publisherId:int}")]
    public async Task<IActionResult> SetPublisher(int bookId, int publisherId)
    {
        var book = await _db.Books.FindAsync(bookId);
        if (book is null) return NotFound();

        var publisher = await _db.Publishers.FindAsync(publisherId);
        if (publisher is null) return NotFound();

        book.PublisherId = publisherId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET: api/books/recent?year=2015
    // Sách xuất bản sau year (mặc định 2015)
    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int year = 2015)
    {
        var q = await _db.Books.AsNoTracking()
                 .Where(b => b.PublishedYear > year)
                 .Select(b => new { b.BookId, b.Title, b.PublishedYear })
                 .ToListAsync();
        return Ok(q);
    }

    // GET: api/books/flat
    // Join phẳng Author–Book–Publisher: AuthorName, BookTitle, PublisherName
    // Dùng SelectMany để EF Core (Pomelo) dịch sang SQL an toàn (tránh CROSS APPLY)
    [HttpGet("flat")]
    public async Task<IActionResult> Flat()
    {
        var q = await _db.Books.AsNoTracking()
                 .SelectMany(b => b.Authors, (b, a) => new
                 {
                     Author = a.Name,
                     Book = b.Title,
                     Publisher = b.Publisher!.Name
                 })
                 .OrderBy(x => x.Author).ThenBy(x => x.Book)
                 .ToListAsync();
        return Ok(q);
    }
}
