using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.WebApi.Extensions;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        // Bootstrap logger so startup failures are logged before the host-configured logger takes over.
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();
            builder.AddBasicHealthChecks();
            builder.RegisterDependencies();
            builder.AddWebApiServices();

            var app = builder.Build();
            app.UseApiPipeline();
            app.Run();
        }
        // HostAbortedException is thrown on purpose by the EF Core tools at design time; it is not a failure.
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
