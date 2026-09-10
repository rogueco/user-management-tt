using System.Net.Http.Json;
using System.Net;

namespace UserManagement.Blazor.Api;

public sealed class ApiResult<T>
{
    public T? Value { get; init; }
    public bool NotFound { get; init; }
    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();

    public bool Succeeded => Value is not null;

    public static async Task<ApiResult<T>> ReadAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new ApiResult<T> { NotFound = true };
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>();
            return new ApiResult<T> { Errors = problem?.Errors ?? new Dictionary<string, string[]>() };
        }

        response.EnsureSuccessStatusCode();
        return new ApiResult<T> { Value = await response.Content.ReadFromJsonAsync<T>() };
    }

    private sealed class ValidationProblem
    {
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
