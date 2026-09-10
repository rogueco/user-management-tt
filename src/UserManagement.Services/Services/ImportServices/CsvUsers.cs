using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using UserManagement.Contracts.Users;

namespace UserManagement.Services.Services.ImportServices;

public sealed record CsvUserRow(int Row, UserRequest? Request, string? Error);

// Expected columns: Forename, Surname, Email, DateOfBirth, IsActive. Rows that cannot be read are
// reported by row number rather than failing the whole file.
public static class CsvUsers
{
    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy"];

    public static IReadOnlyList<CsvUserRow> Parse(string content)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StringReader(content);
        using var csv = new CsvReader(reader, configuration);

        var rows = new List<CsvUserRow>();
        if (!csv.Read() || !csv.ReadHeader())
        {
            return rows;
        }

        while (csv.Read())
        {
            rows.Add(ReadRow(csv));
        }

        return rows;
    }

    private static CsvUserRow ReadRow(CsvReader csv)
    {
        var row = csv.Parser.Row;

        var dateOfBirthText = csv.GetField("DateOfBirth") ?? string.Empty;
        DateOnly? dateOfBirth = DateOnly.TryParseExact(dateOfBirthText, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
        if (dateOfBirth is null && dateOfBirthText.Length > 0)
        {
            return new CsvUserRow(row, null, $"DateOfBirth '{dateOfBirthText}' is not a valid date (use yyyy-MM-dd).");
        }

        var isActiveText = csv.GetField("IsActive") ?? string.Empty;
        var isActive = isActiveText.ToLowerInvariant() switch
        {
            "" or "true" or "yes" or "y" or "1" => true,
            "false" or "no" or "n" or "0" => false,
            _ => (bool?)null
        };
        if (isActive is null)
        {
            return new CsvUserRow(row, null, $"IsActive '{isActiveText}' is not a valid value (use true or false).");
        }

        return new CsvUserRow(row, new UserRequest
        {
            Forename = csv.GetField("Forename") ?? string.Empty,
            Surname = csv.GetField("Surname") ?? string.Empty,
            Email = csv.GetField("Email") ?? string.Empty,
            DateOfBirth = dateOfBirth,
            IsActive = isActive.Value
        }, null);
    }
}
