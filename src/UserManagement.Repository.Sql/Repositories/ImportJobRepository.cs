using Microsoft.EntityFrameworkCore;
using UserManagement.Services.Repositories;
using UserManagement.Domain.Imports;

namespace UserManagement.Repository.Sql.Repositories;

internal sealed class ImportJobRepository : IImportJobRepository
{
    private readonly UserManagementDbContext _db;

    public ImportJobRepository(UserManagementDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task<ImportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.ImportJobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ImportJob>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        => await _db.ImportJobs.OrderByDescending(j => j.CreatedAt).Take(count).ToListAsync(cancellationToken);

    public void Add(ImportJob job) => _db.ImportJobs.Add(job);
}
