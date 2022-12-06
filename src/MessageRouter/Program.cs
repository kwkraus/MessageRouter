using Microsoft.ApplicationInsights.Extensibility;
using MessageRouter.Configuration;
using MessageRouter.Contracts;
using MessageRouter.Services;
using MessageRouter.Rules;
using MessageRouter.Utilities;
using Dapr;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<MessageRouterHealthCheck>("MessageRouterHealthCheck");

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

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/orders", [Topic("orderpubsub", "orders")] (Order order) => {
    Console.WriteLine("Subscriber received : " + order);
    return Results.Ok(order);
});

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public record Order([property: JsonPropertyName("orderId")] int OrderId);