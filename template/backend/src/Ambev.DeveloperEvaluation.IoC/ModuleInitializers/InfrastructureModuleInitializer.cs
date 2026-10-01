using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddPersistence(conn);
    }
}
