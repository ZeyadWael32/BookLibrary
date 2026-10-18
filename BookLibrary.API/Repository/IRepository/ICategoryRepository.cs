using BookLibrary.API.Models;

namespace BookLibrary.API.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category, int>
{
    Task UpdateAsync(Category entity, CancellationToken cancellationToken = default);
}
