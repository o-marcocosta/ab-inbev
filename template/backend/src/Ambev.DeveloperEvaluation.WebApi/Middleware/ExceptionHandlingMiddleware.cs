using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Translates exceptions thrown by the lower layers into consistent HTTP error responses.
/// </summary>
/// <remarks>
/// Expected exceptions (validation, business rules, missing resources) become 4xx responses with a
/// meaningful message. Anything else is logged and returned as a generic 500, without leaking details.
/// </remarks>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (statusCode, error) = Map(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            else
                _logger.LogInformation("Request {Method} {Path} failed with {StatusCode}: {Detail}",
                    context.Request.Method, context.Request.Path, statusCode, error.Detail);

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(error);
        }
    }

    private static (int StatusCode, ApiErrorResponse Error) Map(Exception exception) => exception switch
    {
        ValidationException validation => (StatusCodes.Status400BadRequest,
            ApiErrorResponse.ValidationError(validation.Errors.Select(e => (ValidationErrorDetail)e))),
        DomainException domain => (StatusCodes.Status400BadRequest, new ApiErrorResponse
        {
            Type = "BusinessRuleViolation",
            Error = "Business rule violated",
            Detail = domain.Message
        }),
        ConcurrencyConflictException conflict => (StatusCodes.Status409Conflict, new ApiErrorResponse
        {
            Type = "ConcurrencyConflict",
            Error = "Resource was modified",
            Detail = conflict.Message
        }),
        UnauthorizedAccessException unauthorized => (StatusCodes.Status401Unauthorized, new ApiErrorResponse
        {
            Type = "AuthenticationError",
            Error = "Authentication failed",
            Detail = unauthorized.Message
        }),
        KeyNotFoundException notFound => (StatusCodes.Status404NotFound, new ApiErrorResponse
        {
            Type = "ResourceNotFound",
            Error = "Resource not found",
            Detail = notFound.Message
        }),
        _ => (StatusCodes.Status500InternalServerError, new ApiErrorResponse
        {
            Type = "InternalServerError",
            Error = "An unexpected error occurred",
            Detail = "The server could not process the request. Please try again later."
        })
    };
}
