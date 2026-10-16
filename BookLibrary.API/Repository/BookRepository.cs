using BookLibrary.API.Data;
using BookLibrary.API.Models;
using BookLibrary.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.API.Repository;

public class BookRepository : Repository<Book, int>, IBookRepository
{
    private readonly ApplicationDbContext _db;
    public BookRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task UpdateAsync(Book entity, CancellationToken cancellationToken = default)
    {
        _db.Books.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken = default)
    {
        return await _db.Books.AnyAsync(b => b.Isbn == isbn, cancellationToken);
    }
}
