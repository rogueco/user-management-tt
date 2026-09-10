using System.Net.Http.Json;
using System.Net;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;

namespace UserManagement.Blazor.Api;

public sealed class LogsApi(HttpClient http)
{
    public async Task<PagedResult<LogDto>> GetPageAsync(long? userId, LogAction? actionType, string? search, int page, int pageSize)
        => await http.GetFromJsonAsync<PagedResult<LogDto>>("api/v1/logs" + Query.Build(("userId", userId), ("actionType", actionType), ("search", search), ("page", page), ("pageSize", pageSize)))
           ?? new PagedResult<LogDto>([], 1, 1, 0);

    public async Task<LogDetailsDto?> GetAsync(long id)
    {
        var response = await http.GetAsync($"api/v1/logs/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LogDetailsDto>();
    }
}
