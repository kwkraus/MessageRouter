using Dapr;
using Dapr.Client;
using MessageRouter.Contracts;
using MessageRouter.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace MessageRouter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly DaprClient _dapr;
        private readonly ISchemaValidationService _validator;
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            ISchemaValidationService validationService,
            DaprClient daprClient,
            TelemetryClient telemetryClient,
            ILogger<MessagesController> logger)
        {
            _dapr = daprClient;
            _validator = validationService;
            _telemetry = telemetryClient;
            _logger = logger;
        }

        [Topic("pubsub", "messages")]
        [HttpPost("/routemessage")]
        public async Task<ActionResult> RouteMessage(object order)
        {
            using (var operation = _telemetry.StartOperation<RequestTelemetry>($"{GetType().FullName}::{nameof(RouteMessage)}"))
            {
                try
                {
                    operation.Telemetry.Success = true;
                    var result = await _validator.Validate(order.ToString());
                    _logger.LogInformation(result);

                    if (!string.IsNullOrEmpty(result))
                    {
                        await _dapr.PublishEventAsync<object>("pubsub", "oldOrder", order);
                        await _dapr.PublishEventAsync("pubsub", "unknown-messages");
                    }
                    else
                    {
                        var err = $"Unknown message: \"{order}\"";
                        _logger.LogError(err);
                        throw new InvalidDataException(err);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected exception: {ex}");

                    operation.Telemetry.Success = false;
                    _telemetry.TrackException(ex);

                    throw;
                }
            }

            return Ok(order);
        }

        [HttpGet("/sendmessage")]
        public async Task<ActionResult> SendMessage()
        {
            var randomizer = new Random();
            //var order = new Order(randomizer.Next(1, 100));
            object order;

            if (randomizer.Next(1, 10) % 2 == 0)
            {
                order = new Schema1 { FirstName = "Kevin", LastName = "Kraus" };
            }
            else
            {
                order = new Schema2 { Address = "1234 Main Street", City = "AnyTown", State = "AnyCity", Zip = "12345" };
            }

            // Publish an event/message using Dapr PubSub
            await _dapr.PublishEventAsync<object>("pubsub", "messages", order);
            Console.WriteLine("Published data: " + order);

            return Created("", order);
        }
    }
}
