using Ambev.DeveloperEvaluation.Common.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

public class ApiErrorResponse
{
    public string Type { get; init; } = string.Empty;

    public string Error { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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

    public static ApiErrorResponse FromModelState(ModelStateDictionary modelState) =>
        ValidationError(modelState.SelectMany(entry => entry.Value!.Errors.Select(error => new ValidationErrorDetail
        {
            Error = entry.Key,
            Detail = error.ErrorMessage
        })));
}
