namespace UserManagement.Blazor.Api;

internal static class Query
{
    public static string Build(params (string Key, object? Value)[] parameters)
    {
        var parts = parameters
            .Where(p => p.Value is not null)
            .Select(p => $"{p.Key}={Uri.EscapeDataString(Format(p.Value!))}")
            .ToList();

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private static string Format(object value) => value switch
    {
        bool b => b ? "true" : "false",
        _ => value.ToString() ?? string.Empty
    };
}
