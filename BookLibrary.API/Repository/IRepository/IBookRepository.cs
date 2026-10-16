using BookLibrary.API.Models;

namespace BookLibrary.API.Repository.IRepository;

public interface IBookRepository : IRepository<Book, int>
{
    Task UpdateAsync(Book entity, CancellationToken cancellationToken = default);
    Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken = default);
}
