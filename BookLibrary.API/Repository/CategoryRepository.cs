using BookLibrary.API.Data;
using BookLibrary.API.Models;
using BookLibrary.API.Repository.IRepository;

namespace BookLibrary.API.Repository;

public class CategoryRepository : Repository<Category, int>, ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        _db.Categories.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}