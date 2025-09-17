using AuthorBookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>().ToTable("Authors");
        modelBuilder.Entity<Book>().ToTable("Books");

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
