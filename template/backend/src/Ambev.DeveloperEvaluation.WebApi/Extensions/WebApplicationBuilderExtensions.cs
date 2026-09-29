using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Registers services that depend on the WebApi assembly itself (AutoMapper profiles, Swagger,
    /// error response shape) and therefore cannot live in the IoC project.
    /// </summary>
    public static WebApplicationBuilder AddWebApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

        // Replace System.Text.Json exception text (internal type names, byte positions) with a generic message;
        // the ModelState key still points to the offending field.
        builder.Services.Configure<JsonOptions>(options => options.AllowInputFormatterExceptionMessages = false);

        // Model binding failures (malformed JSON, wrong types) use the same error shape as command validation.
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
                new BadRequestObjectResult(ApiErrorResponse.ValidationError(
                    context.ModelState
                        .Where(entry => entry.Value is { Errors.Count: > 0 })
                        .SelectMany(entry => entry.Value!.Errors.Select(error => new ValidationErrorDetail
                        {
                            Error = entry.Key,
                            Detail = string.IsNullOrEmpty(error.ErrorMessage) ? "The value is invalid." : error.ErrorMessage
                        })))));

        return builder;
    }
}
