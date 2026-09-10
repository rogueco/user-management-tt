using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserManagement.API.Options;

// One Swagger document per discovered API version.
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        options.EnableAnnotations();
        options.SupportNonNullableReferenceTypes();
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "UserManagement.API.xml"), includeControllerXmlComments: true);
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "User Management API",
            Version = description.ApiVersion.ToString(),
            Description = "Manages users, records the actions performed on them, and imports users from CSV files.",
            Contact = new OpenApiContact { Name = "Tom Fletcher", Email = "tom@tomfletcher.tech" }
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}
