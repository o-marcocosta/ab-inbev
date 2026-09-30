using Ambev.DeveloperEvaluation.Application;
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
        builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);
        builder.Services.AddSwaggerGen();
        builder.Services.AddModelBindingErrorResponse();

        return builder;
    }

    /// <summary>
    /// Model binding failures (malformed JSON, wrong types) use the same error shape as command validation.
    /// </summary>
    private static void AddModelBindingErrorResponse(this IServiceCollection services) =>
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
                new BadRequestObjectResult(ApiErrorResponse.FromModelState(context.ModelState)));
}
