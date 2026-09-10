using System.Globalization;

namespace UserManagement.Blazor.Shared;

public static class Formats
{
    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string Timestamp(DateTime timestamp) => timestamp.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";
}
