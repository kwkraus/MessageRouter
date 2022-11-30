namespace MessageRouter.Contracts;

public interface ISchemaValidationService
{
    Task<string> Validate(string input);
}