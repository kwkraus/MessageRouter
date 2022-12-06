using MessageRouter.Utilities;
using NJsonSchema;
using System.Collections.Concurrent;

namespace MessageRouter.Rules;

public static class Utility
{
    public static bool Validate(string input, string schemaFile)
    {
        var schema = _schemas.GetOrAdd(schemaFile, (schemaFile) =>
            AsyncUtil.RunSync<JsonSchema>(() => JsonSchema.FromFileAsync(Path.Combine(References.SchemaDirectory, schemaFile))));

        Console.WriteLine(input);

        return schema.Validate(input).Count == 0;
    }

    private static readonly ConcurrentDictionary<string, JsonSchema> _schemas = new ConcurrentDictionary<string, JsonSchema>();   
}