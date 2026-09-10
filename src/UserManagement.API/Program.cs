using Asp.Versioning.ApiExplorer;
using UserManagement.API.Configuration;
using UserManagement.API.Extensions;
using UserManagement.Messaging.Configuration;
using UserManagement.Repository.Sql.Configuration;
using UserManagement.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddRepositories(options => options.UseUserManagementPostgres(configuration.GetConnectionString("Default")!));
builder.Services.AddPlatformServices();
builder.Services.AddMessaging(configuration);
builder.Services.AddCaching(configuration);
builder.Services.AddPlatformHealthChecks(configuration);
builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddProblemDetails();
builder.Services.AddApiDocumentation();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwaggerWithConfiguration(app.Services.GetRequiredService<IApiVersionDescriptionProvider>());

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });
app.MapFallbackToFile("index.html");

if (configuration.GetValue("Database:InitialiseOnStartup", true))
{
    await app.Services.InitialiseDatabaseAsync();
}

app.Run();

public partial class Program;
