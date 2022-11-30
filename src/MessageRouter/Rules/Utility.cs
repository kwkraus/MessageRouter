using System.Collections.Concurrent;
using NJsonSchema;
using MessageRouter.Utilities;

namespace MessageRouter.Rules;

public static class Utility
{
    public static bool Validate(string input, string schemaFile)
    {
        var schema = _schemas.GetOrAdd(schemaFile, (schemaFile) =>
            AsyncUtil.RunSync<JsonSchema>(() => JsonSchema.FromFileAsync(Path.Combine(References.SchemaDirectory, schemaFile))));
                
        return schema.Validate(input).Count == 0;
    }

    private static ConcurrentDictionary<string, JsonSchema> _schemas = new ConcurrentDictionary<string, JsonSchema>();   
}