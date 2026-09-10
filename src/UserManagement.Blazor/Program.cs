using Material.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserManagement.Blazor;
using UserManagement.Blazor.Api;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<UsersApi>();
builder.Services.AddScoped<LogsApi>();
builder.Services.AddScoped<ImportsApi>();
builder.Services.AddMBServices();

await builder.Build().RunAsync();
