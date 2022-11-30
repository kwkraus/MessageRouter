using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MessageRouter.Services;

public class MessageRouterHealthCheck : IHealthCheck
{
    public MessageRouterHealthCheck(
        ILogger<MessageRouterHealthCheck> logger,
        ServiceBusClient client
    )
    {
        _logger = logger;
        _client = client;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = HealthCheckResult.Healthy("MessageRouter Healthy");
        if (_client.IsClosed)
        {
            var err = "MessageRouter Unhealthy; Service Bus Connection Closed.";
            _logger.LogError(err);

            result = new HealthCheckResult(context.Registration.FailureStatus, err);            
        }
        
        return Task.FromResult(result);
    }

    private readonly ILogger<MessageRouterHealthCheck> _logger;
    private readonly ServiceBusClient _client;
}