using AuthorBookApi.Data;
using AuthorBookApi.Dtos;
using AuthorBookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishersController(AppDbContext db) : ControllerBase
{
    // POST: api/publishers
    [HttpPost]
    public async Task<IActionResult> Create(PublisherCreateDto dto)
    {
        var p = new Publisher { Name = dto.Name, Country = dto.Country };
        db.Publishers.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.PublisherId }, p);
    }

    // GET: api/publishers/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await db.Publishers.FindAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    // LINQ: Publisher có ít nhất N sách (mặc định 3)
    // GET: api/publishers/rich?min=3
    [HttpGet("rich")]
    public async Task<IActionResult> Rich([FromQuery] int min = 3)
    {
        var q = await db.Publishers.AsNoTracking()
                 .Select(p => new { p.PublisherId, p.Name, Count = p.Books.Count })
                 .Where(x => x.Count >= min)
                 .OrderByDescending(x => x.Count)
                 .ToListAsync();
        return Ok(q);
    }
}
