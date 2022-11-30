using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Azure;
using MessageRouter.Configuration;
using MessageRouter.Contracts;
using MessageRouter.Services;
using MessageRouter.Rules;
using MessageRouter.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<MessageRouterHealthCheck>("MessageRouterHealthCheck");

builder.Services.AddAzureClients(acf => {
    acf.ConfigureDefaults(builder.Configuration.GetSection("AzureDefaults"));
    acf.UseCredential(new DefaultAzureCredential());
    acf.AddServiceBusClient(builder.Configuration.GetSection("ServiceBus"));
});

builder.Services.AddHostedService<MessageRouterService>();
builder.Services.AddSingleton<ISchemaValidationService, SchemaValidationService>();
builder.Services.Configure<MessageRouterOptions>(builder.Configuration.GetSection("MessageRouter"));
References.SchemaDirectory = builder.Configuration.GetValue<string>("MessageRouter:SchemaDirectory", string.Empty)!;

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.Configure<TelemetryConfiguration>(config => {
    config.TelemetryInitializers.Add(new TelemetryInitializer());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
