using UserManagement.Domain.Imports;

namespace UserManagement.Services.Repositories;

public interface IImportJobRepository
{
    Task<ImportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportJob>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    void Add(ImportJob job);
}
