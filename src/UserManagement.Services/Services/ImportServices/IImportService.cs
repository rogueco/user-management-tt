using UserManagement.Contracts.Imports;
using UserManagement.Services.Results;

namespace UserManagement.Services.Services.ImportServices;

public interface IImportService
{
    Task<ServiceResult<ImportJobDto>> SubmitAsync(string fileName, string content, CancellationToken cancellationToken = default);
    Task<ServiceResult<ImportJobDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<ImportJobDto>>> GetRecentAsync(CancellationToken cancellationToken = default);
}
