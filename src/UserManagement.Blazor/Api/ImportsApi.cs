using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using UserManagement.Contracts.Imports;

namespace UserManagement.Blazor.Api;

public sealed class ImportsApi(HttpClient http)
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<ImportJobDto> SubmitAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        var stream = new StreamContent(file.OpenReadStream(MaxFileSize));
        stream.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(stream, "file", file.Name);

        var response = await http.PostAsync("api/v1/imports", content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ImportJobDto>())!;
    }

    public async Task<IReadOnlyList<ImportJobDto>> GetRecentAsync()
        => await http.GetFromJsonAsync<IReadOnlyList<ImportJobDto>>("api/v1/imports") ?? [];

    public Task<ImportJobDto?> GetAsync(Guid id)
        => http.GetFromJsonAsync<ImportJobDto>($"api/v1/imports/{id}");
}
