using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;
using UserManagement.Domain.Users;
using UserManagement.Services.Extensions;
using UserManagement.Services.Repositories;
using UserManagement.Services.Results;

namespace UserManagement.Services.Services.UserServices;

public class UserService : IUserService
{
    // Tag removal only invalidates the process that calls it (HybridCache keeps tag timestamps
    // per process), so writes from the API are visible at once and writes from the worker are
    // visible within the short expiry below. Entries with a known key are also removed by key.
    private const string UsersTag = "users";

    private static readonly HybridCacheEntryOptions PageOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(10),
        LocalCacheExpiration = TimeSpan.FromSeconds(10)
    };

    private static readonly HybridCacheEntryOptions UserOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromSeconds(5)
    };

    private readonly IUserRepository _users;
    private readonly IUserLogRepository _logs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly HybridCache _cache;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository users,
        IUserLogRepository logs,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        HybridCache cache,
        ILogger<UserService> logger)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _logs = logs ?? throw new ArgumentNullException(nameof(logs));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetPageAsync(UserFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _cache.GetOrCreateAsync(
            $"users:page:{filter.IsActive?.ToString() ?? "all"}:{page}:{pageSize}",
            async token => (await _users.GetPageAsync(filter, page, pageSize, token)).Map(ToDto),
            PageOptions,
            [UsersTag],
            cancellationToken);

        return ServiceResult<PagedResult<UserDto>>.Success(result);
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _cache.GetOrCreateAsync(
            UserKey(id),
            async token => await _users.GetByIdAsync(id, token) is { } found ? ToDto(found) : null,
            UserOptions,
            [UsersTag],
            cancellationToken);

        return user is null ? NotFound(id) : ServiceResult<UserDto>.Success(user);
    }

    // The log needs the generated id, so the two writes run in one transaction.
    public async Task<ServiceResult<UserDto>> CreateAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        if (!RequestValidation.TryValidate(request, out var message, out var errors))
        {
            return ServiceResult<UserDto>.InvalidInput(message, errors);
        }

        var details = ToDetails(request);
        var created = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var user = User.Create(details);
            _users.Add(user);
            await _unitOfWork.SaveChangesAsync(ct);

            Record(UserLogAction.Created, user, UserLogChange.Between(null, user.Details));
            await _unitOfWork.SaveChangesAsync(ct);

            return ToDto(user);
        }, cancellationToken);

        await _cache.RemoveByTagAsync(UsersTag, cancellationToken);
        return ServiceResult<UserDto>.Success(created);
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(long id, UserRequest request, CancellationToken cancellationToken = default)
    {
        if (!RequestValidation.TryValidate(request, out var message, out var errors))
        {
            return ServiceResult<UserDto>.InvalidInput(message, errors);
        }

        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(id);
        }

        var changes = user.Update(ToDetails(request));
        Record(UserLogAction.Updated, user, changes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await InvalidateAsync(id, cancellationToken);

        return ServiceResult<UserDto>.Success(ToDto(user));
    }

    public async Task<ServiceResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(id);
        }

        _users.Remove(user);
        Record(UserLogAction.Deleted, user, UserLogChange.Between(user.Details, null));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await InvalidateAsync(id, cancellationToken);

        return ServiceResult.Success();
    }

    private ServiceResult<UserDto> NotFound(long id)
    {
        _logger.LogInformation("Unable to find user with id {UserId}", id);
        return ServiceResult<UserDto>.NotFound($"User {id} was not found.");
    }

    private async Task InvalidateAsync(long id, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(UserKey(id), cancellationToken);
        await _cache.RemoveByTagAsync(UsersTag, cancellationToken);
    }

    private static string UserKey(long id) => $"users:{id}";

    private void Record(UserLogAction action, User user, IReadOnlyList<UserLogChange> changes)
        => _logs.Add(UserLog.Create(action, user.Id, user.Details, changes, _timeProvider.GetUtcNow().UtcDateTime));

    private static UserDetails ToDetails(UserRequest request)
        => new(request.Forename, request.Surname, request.Email, request.DateOfBirth!.Value, request.IsActive);

    private static UserDto ToDto(User user)
        => new(user.Id, user.Forename, user.Surname, user.Email, user.DateOfBirth, user.IsActive);
}
