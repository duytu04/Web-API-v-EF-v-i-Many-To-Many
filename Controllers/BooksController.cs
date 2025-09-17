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

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create([FromBody] BookCreateDto dto)
    {
        var entity = new Book { Title = dto.Title.Trim() };
        _db.Books.Add(entity);
        await _db.SaveChangesAsync();
        var result = new BookDto(entity.BookId, entity.Title, new());
        return CreatedAtAction(nameof(GetAll), new { id = entity.BookId }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll()
    {
        var items = await _db.Books
            .Include(b => b.Authors)
            .Select(b => new BookDto(
                b.BookId,
                b.Title,
                b.Authors.Select(a => new AuthorSlimDto(a.AuthorId, a.Name)).ToList()
            ))
            .ToListAsync();

        return Ok(items);
    }
}
