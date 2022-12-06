using MessageRouter.Configuration;
using MessageRouter.Contracts;
using MessageRouter.Rules;
using Microsoft.Extensions.Options;
using RulesEngine.Models;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace MessageRouter.Services;

public class SchemaValidationService : ISchemaValidationService
{
    public SchemaValidationService(
        ILogger<SchemaValidationService> logger,
        IOptions<MessageRouterOptions> options
    )
    {
        _logger = logger;

        using var reader = File.OpenText(options.Value.WorkflowFile);
        var workflowFileContents = reader.ReadToEnd();
        var engineSettings = new ReSettings() { CustomTypes = new[] { typeof(Utility) } };
        _engine = new RulesEngine.RulesEngine(new string[] { workflowFileContents }, engineSettings);
        _workflowName = _engine.GetAllRegisteredWorkflowNames().First();
    }

    public async Task<string> Validate(string input)
    {
        var result = string.Empty;
        var results = await _engine.ExecuteAllRulesAsync(_workflowName, input);    
        results.OnSuccess((eventName) => result = eventName);

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogWarning($"{nameof(Validate)} result was empty. Setting to 'unknown'");
            result = "unknown";
        }
        
        return result;
    }

    private readonly ILogger<SchemaValidationService> _logger;
    private readonly RulesEngine.RulesEngine _engine;
    private readonly string _workflowName;
}