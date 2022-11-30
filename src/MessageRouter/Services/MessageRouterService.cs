using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using MessageRouter.Configuration;
using MessageRouter.Contracts;

namespace MessageRouter.Services;

public class MessageRouterService : BackgroundService
{
    public MessageRouterService(
        ILogger<MessageRouterService> logger,
        TelemetryClient telemetry,
        IOptions<MessageRouterOptions> options,
        ISchemaValidationService validator,
        ServiceBusClient client) 
    {      
        _logger = logger;    
        _telemetry = telemetry;
        _validator = validator;

        _client = client;
        _processor = client.CreateProcessor(options.Value.IngressQueue);
        _processor.ProcessErrorAsync += ProcessServiceBusErrorAsync;
        _processor.ProcessMessageAsync += ProcessServiceBusMessageAsync;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);        
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    } 

    protected Task ProcessServiceBusErrorAsync(ProcessErrorEventArgs args)
    {
        var err = $"Service Bus Error: identifer=\"{args.Identifier}\"; source=\"{args.ErrorSource}\"; ";
        _logger.LogError(err);
        return Task.CompletedTask;
    }

    protected async Task ProcessServiceBusMessageAsync(ProcessMessageEventArgs args)
    {   
        using (var operation = _telemetry.StartOperation<RequestTelemetry>($"{GetType().FullName}::{nameof(this.ProcessServiceBusMessageAsync)}"))
        {
            try
            {
                operation.Telemetry.Success = true;
                if (string.Compare("application/json", args.Message.ContentType, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    var input = args.Message.Body.ToString();
                    var result = await _validator.Validate(input);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var sender = _senders.GetOrAdd(result, (result) => _client.CreateSender(result));
                        var message = new ServiceBusMessage(args.Message);
                        await sender.SendMessageAsync(message);                
                    }
                    else
                    {
                        var err = $"Unknown message: \"{input}\"";              
                        _logger.LogError(err);
                        throw new InvalidDataException(err);
                    }
                }
                else
                {
                    var err = $"Unknown message type: \"{args.Message.ContentType}\"";
                    _logger.LogError(err);
                    throw new InvalidDataException(err);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Unexpected exception: {ex.ToString()}");

                operation.Telemetry.Success = false;
                _telemetry.TrackException(ex);

                throw;
            }
        }
    }

    private readonly ILogger<MessageRouterService> _logger;  
    private readonly TelemetryClient _telemetry;
    private readonly ISchemaValidationService _validator;   
    private readonly ServiceBusClient _client; 
    private readonly ServiceBusProcessor _processor;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new ConcurrentDictionary<string, ServiceBusSender>();
}