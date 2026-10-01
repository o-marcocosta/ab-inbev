using Ambev.DeveloperEvaluation.Messaging;
using Ambev.DeveloperEvaluation.ORM;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOutboxRelayPersistence(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddMessaging(builder.Configuration);

builder.Build().Run();
