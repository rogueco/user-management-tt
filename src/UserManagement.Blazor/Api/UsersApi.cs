using System.Net.Http.Json;
using System.Net;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;

namespace UserManagement.Blazor.Api;

public sealed class UsersApi(HttpClient http)
{
    public async Task<PagedResult<UserDto>> GetPageAsync(bool? isActive, int page, int pageSize)
        => await http.GetFromJsonAsync<PagedResult<UserDto>>("api/v1/users" + Query.Build(("isActive", isActive), ("page", page), ("pageSize", pageSize)))
           ?? new PagedResult<UserDto>([], 1, 1, 0);

    public async Task<UserDto?> GetAsync(long id)
    {
        var response = await http.GetAsync($"api/v1/users/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<ApiResult<UserDto>> CreateAsync(UserRequest request)
        => await ApiResult<UserDto>.ReadAsync(await http.PostAsJsonAsync("api/v1/users", request));

    public async Task<ApiResult<UserDto>> UpdateAsync(long id, UserRequest request)
        => await ApiResult<UserDto>.ReadAsync(await http.PutAsJsonAsync($"api/v1/users/{id}", request));

    public async Task<bool> DeleteAsync(long id)
    {
        var response = await http.DeleteAsync($"api/v1/users/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}
