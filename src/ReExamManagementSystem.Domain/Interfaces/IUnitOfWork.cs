namespace ReExamManagementSystem.Domain.Interfaces;

/// <summary>
/// Coordinates repository instances against a single EF Core change tracker
/// so a service method can touch several aggregates and commit them atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
