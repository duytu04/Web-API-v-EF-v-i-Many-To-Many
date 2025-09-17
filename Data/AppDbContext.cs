using System.Collections.Generic;
using AuthorBookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Publisher> Publishers => Set<Publisher>(); // NEW

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map tên bảng rõ ràng
        modelBuilder.Entity<Author>().ToTable("Authors");
        modelBuilder.Entity<Book>().ToTable("Books");
        modelBuilder.Entity<Publisher>().ToTable("Publishers"); // NEW

        // 1-N: Publisher - Books
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.Restrict); // tránh xoá dây chuyền khi xoá Publisher

        // N-N: Author - Book qua bảng nối AuthorBooks
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Books)
            .WithMany(b => b.Authors)
            .UsingEntity<Dictionary<string, object>>(
                "AuthorBooks",
                right => right.HasOne<Book>()
                              .WithMany()
                              .HasForeignKey("BookId")
                              .OnDelete(DeleteBehavior.Cascade),
                left  => left.HasOne<Author>()
                              .WithMany()
                              .HasForeignKey("AuthorId")
                              .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("AuthorId", "BookId");
                    join.ToTable("AuthorBooks");
                });
    }
}
