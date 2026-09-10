using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Imports;
using UserManagement.Contracts.Logs;
using UserManagement.Contracts.Users;

namespace UserManagement.IntegrationTests.Api;

// No RabbitMQ here: the API hosts the consumer on the in-memory bus, still through the outbox.
public class ImportsApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private HttpClient Client => fixture.Client;

    [Fact]
    public async Task Import_MustRunInTheBackgroundAndReportProgress()
    {
        const string csv = """
            Forename,Surname,Email,DateOfBirth,IsActive
            Ada,Lovelace,ada.lovelace@example.com,1985-12-10,true
            Linus,Torvalds,not-an-email,1969-12-28,true
            Ken,Thompson,ken.thompson@example.com,1983-02-04,false
            """;
        var usersBefore = (await Client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users"))!.TotalCount;

        var submitted = await Client.PostAsync("/api/v1/imports", Multipart(csv));

        submitted.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var job = (await submitted.Content.ReadFromJsonAsync<ImportJobDto>())!;
        submitted.Headers.Location!.ToString().Should().EndWithEquivalentOf($"/api/v1/imports/{job.Id}");
        job.Status.Should().Be(ImportJobStatus.Pending);

        var finished = await WaitForCompletionAsync(job.Id);

        finished.Status.Should().Be(ImportJobStatus.Completed);
        finished.TotalRows.Should().Be(3);
        finished.ProcessedRows.Should().Be(3);
        finished.FailedRows.Should().Be(1);
        finished.Errors.Should().ContainSingle().Which.Row.Should().Be(3);

        (await Client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users"))!.TotalCount.Should().Be(usersBefore + 2);
        var logs = await Client.GetFromJsonAsync<PagedResult<LogDto>>("/api/v1/logs?search=lovelace");
        logs!.Items.Should().ContainSingle().Which.Action.Should().Be(LogAction.Created);

        var recent = await Client.GetFromJsonAsync<IReadOnlyList<ImportJobDto>>("/api/v1/imports");
        recent!.Should().Contain(j => j.Id == job.Id);
    }

    [Fact]
    public async Task Import_WhenFileIsEmpty_MustReturnValidationProblem()
    {
        var response = await Client.PostAsync("/api/v1/imports", Multipart(string.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetImport_WhenUnknown_MustReturnNotFound()
    {
        (await Client.GetAsync($"/api/v1/imports/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<ImportJobDto> WaitForCompletionAsync(Guid id)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            var job = (await Client.GetFromJsonAsync<ImportJobDto>($"/api/v1/imports/{id}"))!;
            if (job.Status is ImportJobStatus.Completed or ImportJobStatus.Failed)
            {
                return job;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("The import did not finish in time.");
    }

    private static MultipartFormDataContent Multipart(string csv)
    {
        var content = new StringContent(csv);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        return new MultipartFormDataContent { { content, "file", "users.csv" } };
    }
}
