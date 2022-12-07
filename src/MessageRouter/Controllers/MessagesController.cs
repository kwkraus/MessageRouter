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

        [Topic("pubsub", "ingress")]
        [HttpPost("/routemessage")]
        public async Task<ActionResult> RouteMessage(object message)
        {
            using (var operation = _telemetry.StartOperation<RequestTelemetry>($"{GetType().FullName}::{nameof(RouteMessage)}"))
            {
                try
                {
                    operation.Telemetry.Success = true;
                    var result = await _validator.Validate(message.ToString());
                    _logger.LogInformation($"Validated message is '{result}'");

                    if (!string.IsNullOrEmpty(result))
                    {
                        await _dapr.PublishEventAsync<object>("pubsub", result, message);
                    }
                    else
                    {
                        var err = $"Validation failed for: \"{message}\"";
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

            return Ok(message);
        }

        [HttpGet("/sendrandommessage")]
        public async Task<ActionResult> SendRandomMessage()
        {
            var randomizer = new Random();
            object message;

            //randomized number used to determine which message to send
            var randomNum = randomizer.Next(1, 10);

            if (randomNum % 2 == 0) //schema01
            {
                message = new Schema1 { FirstName = "Kevin", LastName = "Kraus" };
            }
            else if(randomNum % 5 == 0) //unknown
            {
                message = new { Age = 122, BirthDay = "Jan 1 1900" };
            }
            else //schema02
            {
                message = new Schema2 { Address = "1234 Main Street", City = "AnyTown", State = "AnyCity", Zip = "12345" };
            }

            // Publish an event/message using Dapr PubSub
            await _dapr.PublishEventAsync<object>("pubsub", "ingress", message);
            _logger.LogInformation($"{nameof(SendRandomMessage)} Published data: {message}");

            return Created("", message);
        }
    }
}