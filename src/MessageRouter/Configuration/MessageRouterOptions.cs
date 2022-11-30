namespace MessageRouter.Configuration;

public class MessageRouterOptions
{
    public string IngressQueue { get; set; } = string.Empty;

    public string WorkflowFile { get; set; } = string.Empty;

    public string SchemaDirectory { get; set; } = string.Empty;
}