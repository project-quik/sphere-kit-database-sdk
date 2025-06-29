using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0618 // Type or member is obsolete

#nullable enable
namespace SphereKit
{
    public class GeoJsonValidationException : Exception
    {
        public GeoJsonValidationException(string message) : base(message)
        {
        }
    }

    public static class GeoJsonValidator
    {
        private const string position = @"{
            ""type"": ""array"",
            ""minItems"": 2,
            ""items"": {
            ""type"": ""number""
            },
        }";

        private static readonly string _point = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^Point$""
                }},
                ""coordinates"": {position}
            }},
            ""required"": [""type"", ""coordinates""],
            ""additionalProperties"": false
        }}";

        private static readonly string _multiPoint = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^MultiPoint$""
                }},
                ""coordinates"": {{
                    ""type"": ""array"",
                    ""minItems"": 2,
                    ""items"": {position}
                }}
            }},
            ""required"": [""type"", ""coordinates""],
            ""additionalProperties"": false
        }}";

        private static readonly string _lineString = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^LineString$""
                }},
                ""coordinates"": {{
                    ""type"": ""array"",
                    ""minItems"": 2,
                    ""items"": {position}
                }}
            }},
            ""required"": [""type"", ""coordinates""],
            ""additionalProperties"": false
        }}";

        private static readonly string _multiLineString = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^MultiLineString$""
                }},
                ""coordinates"": {{
                    ""type"": ""array"",
                    ""items"": {{
                        ""type"": ""array"",
                        ""minItems"": 2,
                        ""items"": {position}
                    }}
                }}
            }},
            ""required"": [""type"", ""coordinates""],
            ""additionalProperties"": false
        }}";

        private static readonly string _polygon = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^Polygon$""
                }},
                ""coordinates"": {{
                    ""type"": ""array"",
                    ""items"": {{
                        ""type"": ""array"",
                        ""minItems"": 4,
                        ""items"": {position}
                    }}
                }}
            }},
            ""required"": [""type"", ""coordinates""],
            ""additionalProperties"": false
        }}";

        private static readonly string _multiPolygon = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^MultiPolygon$""
                }},
                ""coordinates"": {{
                    ""type"": ""array"",
                    ""items"": {{
                        ""type"": ""array"",
                        ""items"": {{
                            ""type"": ""array"",
                            ""minItems"": 4,
                            ""items"": {position}
                        }}
                    }}
                }}
            }},
            ""required"": [""type"", ""coordinates""],
            ""additionalProperties"": false
        }}";

        private static readonly string _geometryCollection = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""type"": {{
                    ""type"": ""string"",
                    ""pattern"": ""^GeometryCollection$""
                }},
                ""geometries"": {{
                    ""type"": ""array"",
                    ""items"": {{
                        ""oneOf"": [
                            {_point},
                            {_multiPoint},
                            {_lineString},
                            {_multiLineString},
                            {_polygon},
                            {_multiPolygon},
                            {_geometryCollection}
                        ] 
                    }}
                }}
            }},
            ""required"": [""type"", ""geometries""],
            ""additionalProperties"": false
        }}";


        private static readonly Dictionary<string, JsonSchema> _allGeojsonTypes = new()
        {
            { "Point", JsonSchemaParser.Parse(_point) },
            { "MultiPoint", JsonSchemaParser.Parse(_multiPoint) },
            { "LineString", JsonSchemaParser.Parse(_lineString) },
            { "MultiLineString", JsonSchemaParser.Parse(_multiLineString) },
            { "Polygon", JsonSchemaParser.Parse(_polygon) },
            { "MultiPolygon", JsonSchemaParser.Parse(_multiPolygon) },
            { "GeometryCollection", JsonSchemaParser.Parse(_geometryCollection) }
        };

        public static void Validate(object value, string[]? supportedTypes = null)
        {
            if (value is not Dictionary<string, object> geojsonDict)
                throw new GeoJsonValidationException("Invalid GeoJSON object.");

            var testGeojson = JObject.FromObject(geojsonDict);

            if (!testGeojson.TryGetValue("type", out var typeToken))
                throw new GeoJsonValidationException("GeoJSON object must have a 'type' property.");

            var geojsonTypes = _allGeojsonTypes;
            if (supportedTypes != null)
                geojsonTypes = supportedTypes.ToDictionary(type => type, type => geojsonTypes[type]);

            var type = typeToken.ToString();
            if (!geojsonTypes.ContainsKey(type))
                throw new GeoJsonValidationException($"\"{type}\" is not a valid GeoJSON type for this operation.");

            bool isValid;

            try
            {
                var schema = geojsonTypes[type];
                isValid = schema.Validate(testGeojson);
            }
            catch (Exception ex)
            {
                throw new GeoJsonValidationException("Invalid GeoJSON object: " + ex.Message);
            }

            if (!isValid) throw new GeoJsonValidationException("Invalid GeoJSON object.");

            if (type == "Polygon") ValidatePolygonCoordinates(testGeojson);
        }

        private static void ValidatePolygonCoordinates(JObject polygon)
        {
            if (!polygon.TryGetValue("coordinates", out var coordinatesToken) ||
                coordinatesToken.Type != JTokenType.Array)
                throw new GeoJsonValidationException("Polygon must have 'coordinates' array.");

            foreach (var ring in coordinatesToken)
            {
                var ringArray = ring as JArray;
                if (ringArray == null || ringArray.Count < 4)
                    throw new GeoJsonValidationException("Each ring of a Polygon must have at least 4 coordinates.");

                var firstPoint = ringArray.First as JArray;
                var lastPoint = ringArray.Last as JArray;
                if (!ArePointsEqual(firstPoint!, lastPoint!))
                    throw new GeoJsonValidationException("A Polygon's first and last points must be equivalent.");
            }
        }

        private static bool ArePointsEqual(JArray point1, JArray point2)
        {
            if (point1.Count != point2.Count) return false;

            return !point1.Where((t, i) => !JToken.DeepEquals(t, point2[i])).Any();
        }
    }
}