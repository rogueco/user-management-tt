using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Domain.Users;
using UserManagement.Services.Extensions;
using UserManagement.Services.Repositories;
using UserManagement.Services.Results;

namespace UserManagement.Services.Services.LogServices;

public class UserLogService : IUserLogService
{
    // A log entry never changes once written. Whether its user still exists can, so that is read live.
    private static readonly HybridCacheEntryOptions LogOptions = new()
    {
        Expiration = TimeSpan.FromHours(1),
        LocalCacheExpiration = TimeSpan.FromHours(1)
    };

    private readonly IUserLogRepository _logs;
    private readonly IUserRepository _users;
    private readonly HybridCache _cache;
    private readonly ILogger<UserLogService> _logger;

    public UserLogService(IUserLogRepository logs, IUserRepository users, HybridCache cache, ILogger<UserLogService> logger)
    {
        _logs = logs ?? throw new ArgumentNullException(nameof(logs));
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceResult<PagedResult<LogDto>>> GetPageAsync(LogFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
        => ServiceResult<PagedResult<LogDto>>.Success((await _logs.GetPageAsync(filter, page, pageSize, cancellationToken)).Map(ToDto));

    public async Task<ServiceResult<LogDetailsDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var key = $"logs:{id}";
        var cached = await _cache.GetOrCreateAsync(
            key,
            async token => await _logs.GetByIdAsync(id, token) is { } log ? CachedLog.From(log) : null,
            LogOptions,
            cancellationToken: cancellationToken);

        if (cached is null)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Unable to find log entry with id {LogId}", id);
            return ServiceResult<LogDetailsDto>.NotFound($"Log entry {id} was not found.");
        }

        var userExists = await _users.ExistsAsync(cached.Log.UserId, cancellationToken);
        return ServiceResult<LogDetailsDto>.Success(new LogDetailsDto(cached.Log, cached.Changes, userExists));
    }

    public static LogDto ToDto(UserLog log)
        => new(log.Id, log.UserId, $"{log.Forename} {log.Surname}", log.Email, log.Action.ToContract(), log.Timestamp);

    private sealed record CachedLog(LogDto Log, List<LogChangeDto> Changes)
    {
        public static CachedLog From(UserLog log)
            => new(ToDto(log), log.Changes.Select(c => new LogChangeDto(c.Field, c.Before, c.After)).ToList());
    }
}
