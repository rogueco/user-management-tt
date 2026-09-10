using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.Results;

public static class RequestValidation
{
    // Runs the data annotations on a request and shapes the failures the same way MVC does.
    public static bool TryValidate(object request, out string message, out IReadOnlyDictionary<string, string[]> errors)
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true))
        {
            message = string.Empty;
            errors = new Dictionary<string, string[]>();
            return true;
        }

        errors = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), (r, member) => (member, r.ErrorMessage ?? "Invalid value."))
            .GroupBy(e => e.member)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Item2).ToArray());
        message = results[0].ErrorMessage ?? "The request is invalid.";
        return false;
    }
}
