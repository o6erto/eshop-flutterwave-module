using System.Collections.Generic;

namespace Eshop.Modules.Flutterwave.Abstractions;

public enum ResultErrorCode
{
    None = 0,
    NotFound = 1,
    ValidationFailure = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    BadRequest = 6,
    InternalError = 7
}

public class Result
{
    public bool Success { get; protected set; }
    public ResultErrorCode ErrorCode { get; protected set; }
    public string ErrorMessage { get; protected set; } = string.Empty;
    public IEnumerable<string>? ValidationErrors { get; protected set; }

    protected Result() { }

    public static Result Ok() => new() { Success = true, ErrorCode = ResultErrorCode.None };
    
    public static Result Failed(ResultErrorCode errorCode, string message, IEnumerable<string>? validationErrors = null) => new() 
    { 
        Success = false, 
        ErrorCode = errorCode, 
        ErrorMessage = message,
        ValidationErrors = validationErrors
    };
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result() { }

    public static Result<T> SuccessResult(T data) => new() 
    { 
        Success = true, 
        ErrorCode = ResultErrorCode.None,
        Data = data 
    };

    public static new Result<T> Failed(ResultErrorCode errorCode, string message, IEnumerable<string>? validationErrors = null) => new() 
    { 
        Success = false, 
        ErrorCode = errorCode, 
        ErrorMessage = message,
        ValidationErrors = validationErrors
    };
}
