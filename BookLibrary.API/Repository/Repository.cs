using BookLibrary.API.Data;
using BookLibrary.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookLibrary.API.Repository;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    private readonly ApplicationDbContext _db;
    internal readonly DbSet<TEntity> dbSet;
    public Repository(ApplicationDbContext db)
    {
        _db = db;
        dbSet = _db.Set<TEntity>(); 
    }
    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbSet.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        dbSet.Remove(entity);
        await SaveAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, string? includeProperties = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>>? filter = null, bool tracked = true, string? includeProperties = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = dbSet;
        if (!tracked)
        {
            query = query.AsNoTracking();
        }
        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await dbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
