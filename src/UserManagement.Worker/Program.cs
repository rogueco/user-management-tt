using UserManagement.Messaging.Configuration;
using UserManagement.Repository.Sql.Configuration;
using UserManagement.Services.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPlatformServices()
    .AddCaching(builder.Configuration)
    .AddRepositories(options => options.UseUserManagementPostgres(builder.Configuration.GetConnectionString("Default")!))
    .AddMessaging(builder.Configuration);

await builder.Build().RunAsync();
