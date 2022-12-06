using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MessageRouter.Services;

public class MessageRouterHealthCheck : IHealthCheck
{
    public MessageRouterHealthCheck(
        ILogger<MessageRouterHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = HealthCheckResult.Healthy("MessageRouter Healthy");
        
        return Task.FromResult(result);
    }

    private readonly ILogger<MessageRouterHealthCheck> _logger;
}