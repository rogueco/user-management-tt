using Asp.Versioning.ApiExplorer;

namespace UserManagement.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public const string ApiName = "usermanagementapi";

    public static IApplicationBuilder UseSwaggerWithConfiguration(this IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    {
        app.UseSwagger(options => options.RouteTemplate = $"{ApiName}/{{documentName}}/swagger.json");
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = ApiName;
            foreach (var groupName in apiVersionDescriptionProvider.ApiVersionDescriptions.Select(description => description.GroupName))
            {
                options.SwaggerEndpoint($"/{ApiName}/{groupName}/swagger.json", groupName.ToUpperInvariant());
            }
        });

        return app;
    }
}
