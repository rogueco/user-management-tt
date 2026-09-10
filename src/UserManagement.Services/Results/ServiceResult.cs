namespace UserManagement.Services.Results;

// The outcome of a service call. Controllers map the code to a status; they never inspect the data.
public class ServiceResult
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    protected ServiceResult(ServiceResultCode resultCode, string? errorMessage, IReadOnlyDictionary<string, string[]>? errors)
    {
        ResultCode = resultCode;
        ErrorMessage = errorMessage;
        Errors = errors ?? NoErrors;
    }

    public ServiceResultCode ResultCode { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
    public bool IsSuccessful => ResultCode == ServiceResultCode.Success;

    public static ServiceResult Success() => new(ServiceResultCode.Success, null, null);
    public static ServiceResult NotFound(string message) => new(ServiceResultCode.NotFound, message, null);
    public static ServiceResult InvalidInput(string message, IReadOnlyDictionary<string, string[]>? errors = null) => new(ServiceResultCode.InvalidInput, message, errors);
    public static ServiceResult InternalError(string message) => new(ServiceResultCode.InternalError, message, null);
}

public sealed class ServiceResult<T> : ServiceResult
{
    private ServiceResult(ServiceResultCode resultCode, T? value, string? errorMessage, IReadOnlyDictionary<string, string[]>? errors)
        : base(resultCode, errorMessage, errors)
        => Value = value;

    public T? Value { get; }

    public static ServiceResult<T> Success(T value) => new(ServiceResultCode.Success, value, null, null);
    public static new ServiceResult<T> NotFound(string message) => new(ServiceResultCode.NotFound, default, message, null);
    public static new ServiceResult<T> InvalidInput(string message, IReadOnlyDictionary<string, string[]>? errors = null) => new(ServiceResultCode.InvalidInput, default, message, errors);
    public static new ServiceResult<T> InternalError(string message) => new(ServiceResultCode.InternalError, default, message, null);
}
