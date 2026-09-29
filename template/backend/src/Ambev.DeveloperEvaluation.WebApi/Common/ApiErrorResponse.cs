using Ambev.DeveloperEvaluation.Common.Validation;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

public class ApiErrorResponse
{
    public string Type { get; init; } = string.Empty;

    public string Error { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    public IEnumerable<ValidationErrorDetail>? Errors { get; init; }

    /// <summary>
    /// Builds the single error shape used for every invalid input, whether it failed model binding
    /// or a command validator.
    /// </summary>
    public static ApiErrorResponse ValidationError(IEnumerable<ValidationErrorDetail> errors)
    {
        var list = errors.ToList();

        return new ApiErrorResponse
        {
            Type = "ValidationError",
            Error = "Invalid input data",
            Detail = string.Join(" ", list.Select(e => e.Detail).Distinct()),
            Errors = list
        };
    }
}
