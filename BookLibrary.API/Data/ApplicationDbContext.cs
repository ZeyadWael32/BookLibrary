using BookLibrary.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Isbn).IsRequired().HasMaxLength(13);
            entity.Property(e => e.PublishedYear).IsRequired();
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasIndex(e => e.Isbn).IsUnique();
        });
    }
}
