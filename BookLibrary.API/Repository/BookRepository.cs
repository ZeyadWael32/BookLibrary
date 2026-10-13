using BookLibrary.API.Data;
using BookLibrary.API.Models;
using BookLibrary.API.Repository.IRepository;

namespace BookLibrary.API.Repository
{
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
    }
}
