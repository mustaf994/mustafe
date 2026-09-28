using System.Collections.Concurrent;
using ReExamManagementSystem.Domain.Interfaces;
using ReExamManagementSystem.Infrastructure.Data;

namespace ReExamManagementSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(T),
            _ => new GenericRepository<T>(_context));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        if (_disposed) return;
        _context.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
