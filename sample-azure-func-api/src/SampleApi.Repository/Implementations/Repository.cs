using Microsoft.EntityFrameworkCore;
using SampleApi.Entities.Base;
using SampleApi.Repository.Interfaces;

namespace SampleApi.Repository.Implementations;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly DbContext.ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public Repository(DbContext.ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
