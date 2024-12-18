using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

#nullable enable
namespace SphereKit
{
    internal abstract class JsonSchema
    {
        public abstract bool Validate(JToken token);
    }

    internal class JsonObjectSchema : JsonSchema
    {
        public readonly Dictionary<string, JsonSchema> Properties = new();
        public List<string> Required = new();
        private readonly bool _additionalProperties;

        public JsonObjectSchema(bool additionalProperties = false)
        {
            _additionalProperties = additionalProperties;
        }

        public override bool Validate(JToken token)
        {
            if (token.Type != JTokenType.Object)
                throw new InvalidExpressionException($"Expected object, got {token.Type} ({token}).");
            var obj = (JObject)token;

            var missingKeys = Required.Where(requiredProp => !obj.ContainsKey(requiredProp)).ToList();
            if (missingKeys.Count > 0)
                throw new InvalidExpressionException(
                    $"Object {obj} is missing the keys ({string.Join(", ", missingKeys)}) of the required keys ({string.Join(", ", Required)}).");

            foreach (var prop in Properties)
                if (obj.TryGetValue(prop.Key, out var value))
                    if (!prop.Value.Validate(value))
                        return false;

            if (_additionalProperties) return true;

            var unexpectedProperties = obj.Properties().Select(p => p.Name).Where(name => !Properties.ContainsKey(name))
                .ToArray();
            if (unexpectedProperties.Any())
                throw new InvalidExpressionException(
                    $"Object {obj} has unexpected properties: ({string.Join(", ", unexpectedProperties)})");
            return true;
        }
    }

    internal class JsonArraySchema : JsonSchema
    {
        private readonly JsonSchema _items;
        private readonly int _minItems;

        public JsonArraySchema(JsonSchema items, int minItems = 0)
        {
            _items = items;
            _minItems = minItems;
        }

        public override bool Validate(JToken token)
        {
            if (token.Type != JTokenType.Array)
                throw new InvalidExpressionException($"Expected array, got {token.Type} ({token}).");
            var array = (JArray)token;
            if (array.Count < _minItems)
                throw new InvalidExpressionException(
                    $"Array {array} has fewer items ({array.Count}) than the minimum required ({_minItems}).");

            return array.All(item => _items.Validate(item));
        }
    }

    internal class JsonMultiSchema : JsonSchema
    {
        private readonly JsonSchema[] _schemas;
        private readonly JsonMultiSchemaType _type;

        public JsonMultiSchema(JsonSchema[] schemas, JsonMultiSchemaType type)
        {
            _schemas = schemas;
            _type = type;
        }

        public override bool Validate(JToken token)
        {
            switch (_type)
            {
                case JsonMultiSchemaType.OneOf:
                    var isValid = _schemas.Any(schema =>
                    {
                        try
                        {
                            schema.Validate(token);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    });
                    if (!isValid)
                        throw new InvalidExpressionException(
                            $"Token {token} does not match any of the schemas.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }
    }

    internal enum JsonMultiSchemaType
    {
        OneOf
    }

    internal class JsonNumberSchema : JsonSchema
    {
        public override bool Validate(JToken token)
        {
            if (token.Type is not (JTokenType.Float or JTokenType.Integer))
                throw new InvalidExpressionException($"Expected number, got {token.Type} ({token}).");

            return true;
        }
    }

    internal class JsonStringSchema : JsonSchema
    {
        private readonly string? _pattern;

        public JsonStringSchema(string? pattern)
        {
            _pattern = pattern;
        }

        public override bool Validate(JToken token)
        {
            if (token.Type != JTokenType.String)
                throw new InvalidExpressionException($"Expected string, got {token.Type} ({token}).");
            var str = (string)token!;

            if (string.IsNullOrEmpty(_pattern)) return true;

            if (!System.Text.RegularExpressions.Regex.IsMatch(str, _pattern))
                throw new InvalidExpressionException($"String '{str}' does not match pattern '{_pattern}'.");

            return true;
        }
    }

    internal static class JsonSchemaParser
    {
        public static JsonSchema Parse(string schemaJson)
        {
            var schemaObj = JObject.Parse(schemaJson);
            return ParseSchema(schemaObj);
        }

        private static JsonSchema ParseSchema(JObject schemaObj)
        {
            var type = schemaObj["type"]?.ToString();
            return type switch
            {
                "object" => ParseObjectSchema(schemaObj),
                "array" => ParseArraySchema(schemaObj),
                "number" => new JsonNumberSchema(),
                "string" => new JsonStringSchema(schemaObj["pattern"]?.ToString()),
                _ => throw new NotSupportedException($"Unsupported schema type: {type}")
            };
        }

        private static JsonObjectSchema ParseObjectSchema(JObject schemaObj)
        {
            var schema = new JsonObjectSchema(schemaObj["additionalProperties"]?.ToObject<bool>() ?? true);

            if (schemaObj["properties"] is JObject properties)
                foreach (var prop in properties)
                    schema.Properties[prop.Key] = ParseSchema((JObject)prop.Value!);

            if (schemaObj["required"] is JArray required) schema.Required = required.Select(r => r.ToString()).ToList();

            return schema;
        }

        private static JsonArraySchema ParseArraySchema(JObject schemaObj)
        {
            var minItems = schemaObj["minItems"]?.ToObject<int>() ?? 0;
            var items = (JObject)schemaObj["items"]!;

            if (items.TryGetValue("oneOf", out var schemas))
                return new JsonArraySchema(
                    minItems: minItems,
                    items: new JsonMultiSchema(
                        ((JArray)schemas).Select(s => ParseSchema((JObject)s!)).ToArray(),
                        JsonMultiSchemaType.OneOf
                    )
                );

            return new JsonArraySchema(
                minItems: minItems,
                items: ParseSchema(items)
            );
        }
    }
}