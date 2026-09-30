using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

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
        builder.Services.AddSwaggerWithBearerAuth();
        builder.Services.AddUnauthorizedErrorResponse();
        builder.Services.AddModelBindingErrorResponse();

        return builder;
    }

    private static void AddSwaggerWithBearerAuth(this IServiceCollection services)
    {
        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Description = "Token returned by POST /api/auth."
        };

        var bearerReference = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
        };

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerReference] = Array.Empty<string>() });
        });
    }

    /// <summary>
    /// Unauthenticated requests get the same error shape as every other failure instead of an empty 401.
    /// </summary>
    private static void AddUnauthorizedErrorResponse(this IServiceCollection services) =>
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            options.Events = new JwtBearerEvents { OnChallenge = WriteUnauthorizedAsync });

    private static Task WriteUnauthorizedAsync(JwtBearerChallengeContext context)
    {
        var error = new ApiErrorResponse
        {
            Type = "AuthenticationError",
            Error = "Unauthorized",
            Detail = "A valid bearer token is required."
        };

        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(error);
    }

    /// <summary>
    /// Model binding failures (malformed JSON, wrong types) use the same error shape as command validation.
    /// </summary>
    private static void AddModelBindingErrorResponse(this IServiceCollection services) =>
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
                new BadRequestObjectResult(ApiErrorResponse.FromModelState(context.ModelState)));
}
