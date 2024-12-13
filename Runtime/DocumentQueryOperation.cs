using System;
using System.Collections.Generic;

#nullable enable
namespace SphereKit
{
    public class DocumentQueryOperation
    {
        public readonly DocumentQueryOperationType OperationType;
        public readonly string? Field;
        public readonly object Value;

        private DocumentQueryOperation(DocumentQueryOperationType operationType, string? field, object value)
        {
            OperationType = operationType;
            Field = field;
            Value = value;
        }

        private static void CheckNumberValue(object value)
        {
            if (value is not long && value is not int && value is not float && value is not double)
                throw new ArgumentException("Value must be a number (long/int/float/double).");
        }

        private static void CheckArrayValue(object value)
        {
            if (value is not Array && value is not List<object>)
                throw new ArgumentException("Value must be an array or list.");
        }

        public static DocumentQueryOperation And(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.And, null, queries);
        }

        public static DocumentQueryOperation Nor(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Nor, null, queries);
        }

        public static DocumentQueryOperation Or(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Or, null, queries);
        }

        public static DocumentQueryOperation Equal(string? field, object value)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Equal, field, value);
        }

        public static DocumentQueryOperation NotEqual(string? field, object value)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.NotEqual, field, value);
        }

        public static DocumentQueryOperation GreaterThan(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.GreaterThan, field, value);
        }

        public static DocumentQueryOperation GreaterThanOrEqual(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.GreaterThanOrEqual, field, value);
        }

        public static DocumentQueryOperation LessThan(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.LessThan, field, value);
        }

        public static DocumentQueryOperation LessThanOrEqual(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.LessThanOrEqual, field, value);
        }

        public static DocumentQueryOperation In(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckArrayValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.In, field, value);
        }

        public static DocumentQueryOperation NotIn(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckArrayValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.NotIn, field, value);
        }

        public static DocumentQueryOperation Exists(string? field)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Exists, field, true);
        }

        public static DocumentQueryOperation NotExists(string? field)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Exists, field, false);
        }

        public static DocumentQueryOperation DataTypeIs(string? field, SphereKitDataType type)
        {
            var typeString = type switch
            {
                SphereKitDataType.String => "string",
                SphereKitDataType.Number => "number",
                SphereKitDataType.Boolean => "boolean",
                SphereKitDataType.List => "array",
                SphereKitDataType.Dictionary => "object",
                SphereKitDataType.Null => "null",
                _ => throw new ArgumentOutOfRangeException()
            };

            return new DocumentQueryOperation(DocumentQueryOperationType.DataTypeIs, field, typeString);
        }

        public static DocumentQueryOperation Modulo(string? field, object divisor, object remainder)
        {
            CheckNumberValue(divisor);
            if ((int)divisor == 0)
                throw new ArgumentException("Cannot divide by zero.");

            CheckNumberValue(remainder);

            return new DocumentQueryOperation(DocumentQueryOperationType.Modulo, field, new[] { divisor, remainder });
        }

        public static DocumentQueryOperation MatchesRegex(string? field, string pattern)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.MatchesRegex, field, pattern);
        }

        public static DocumentQueryOperation GeoIntersects(string? field, object geometry)
        {
            GeoJsonValidator.Validate(geometry);

            return new DocumentQueryOperation(DocumentQueryOperationType.GeoIntersects, field,
                new Dictionary<string, object>
                {
                    { "$geometry", geometry }
                });
        }

        public static DocumentQueryOperation GeoWithin(string? field, object geometry)
        {
            GeoJsonValidator.Validate(geometry);

            return new DocumentQueryOperation(DocumentQueryOperationType.GeoWithin, field,
                new Dictionary<string, object>
                {
                    { "$geometry", geometry }
                });
        }

        public static DocumentQueryOperation GeoNear(string? field, object geometry, long? minDistance = null,
            long? maxDistance = null)
        {
            GeoJsonValidator.Validate(geometry);

            var operatorValue = new Dictionary<string, object>
            {
                { "$geometry", geometry }
            };

            if (minDistance != null)
            {
                CheckNumberValue(minDistance);
                operatorValue.Add("$minDistance", minDistance);
            }

            if (maxDistance != null)
            {
                CheckNumberValue(maxDistance);
                operatorValue.Add("$maxDistance", maxDistance);
            }

            return new DocumentQueryOperation(DocumentQueryOperationType.GeoNear, field, operatorValue);
        }

        public static DocumentQueryOperation ContainsAllOf(string? field, object values)
        {
            CheckArrayValue(values);

            return new DocumentQueryOperation(DocumentQueryOperationType.ContainsAllOf, field, values);
        }

        public static DocumentQueryOperation ElementMatches(string field, DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.ElementMatches, field, queries);
        }

        public static DocumentQueryOperation ArraySizeIs(string? field, long size)
        {
            CheckNumberValue(size);

            return new DocumentQueryOperation(DocumentQueryOperationType.ArraySizeIs, field, size);
        }

        private static string GetStringOperationType(DocumentQueryOperationType operationType)
        {
            return operationType switch
            {
                DocumentQueryOperationType.Equal => "$eq",
                DocumentQueryOperationType.NotEqual => "$ne",
                DocumentQueryOperationType.GreaterThan => "$gt",
                DocumentQueryOperationType.GreaterThanOrEqual => "$gte",
                DocumentQueryOperationType.LessThan => "$lt",
                DocumentQueryOperationType.LessThanOrEqual => "$lte",
                DocumentQueryOperationType.In => "$in",
                DocumentQueryOperationType.NotIn => "$nin",
                DocumentQueryOperationType.Exists => "$exists",
                DocumentQueryOperationType.DataTypeIs => "$type",
                DocumentQueryOperationType.Modulo => "$mod",
                DocumentQueryOperationType.MatchesRegex => "$regex",
                DocumentQueryOperationType.GeoIntersects => "$geoIntersects",
                DocumentQueryOperationType.GeoWithin => "$geoWithin",
                DocumentQueryOperationType.GeoNear => "nearSphere",
                DocumentQueryOperationType.ContainsAllOf => "$all",
                DocumentQueryOperationType.ElementMatches => "$elemMatch",
                DocumentQueryOperationType.ArraySizeIs => "$size",
                DocumentQueryOperationType.And => "$and",
                DocumentQueryOperationType.Nor => "$nor",
                DocumentQueryOperationType.Or => "$or",
                _ => ""
            };
        }

        /// <summary>
        /// This method converts a dictionary of queries into a dictionary of request data that can be sent to the server.
        /// </summary>
        /// <param name="queries">A dictionary of queries to convert into request data.</param>
        /// <param name="isElementMatches">Whether the query is in an element match query</param>
        /// <returns>A dictionary of request data that can be sent to the server.</returns>
        internal static Dictionary<string, object> ConvertQueryToRequestData(DocumentQueryOperation[] queries,
            bool isElementMatches = false)
        {
            // This method resolves the value of an operation in a query to a format acceptable by the server.
            //
            // Parameters:
            // - operationValue: The value of the operation.
            //
            // Returns: The resolved value of the operation to be added to the request data.
            object ResolveOperationValue(DocumentQueryOperationType operationType, object operationValue)
            {
                switch (operationValue)
                {
                    // Convert a list of projections into the $eval operation.
                    case DocumentQueryProjection[] or List<DocumentQueryProjection>:
                    {
                        var evalOperationValue = new Dictionary<string, object>
                        {
                            { "$eval", new List<string>() }
                        };
                        List<DocumentQueryProjection> projections;
                        if (operationValue is DocumentQueryProjection[] projectionArray)
                            projections = new List<DocumentQueryProjection>(projectionArray);
                        else
                            projections = (List<DocumentQueryProjection>)operationValue;

                        foreach (var projection in projections)
                            ((List<string>)evalOperationValue["$eval"]).Add($"${projection.Field}");

                        operationValue = evalOperationValue;
                        break;
                    }
                    // Convert a single projection into the $eval operation.
                    case DocumentQueryProjection projection:
                    {
                        var evalOperationValue = new Dictionary<string, object>
                        {
                            { "$eval", $"${projection.Field}" }
                        };
                        operationValue = evalOperationValue;
                        break;
                    }
                    // Convert an array of operations into a list of dictionaries with the operation type as the key, or a dictionary of operations if these are below an element match.
                    case DocumentQueryOperation[] queryList:
                    {
                        if (operationType == DocumentQueryOperationType.ElementMatches)
                        {
                            operationValue = ConvertQueryToRequestData(queryList, true);
                            break;
                        }

                        operationValue = Array.ConvertAll(queryList, eachQuery =>
                        {
                            var isLogicalOperation = eachQuery.OperationType is DocumentQueryOperationType.And
                                or DocumentQueryOperationType.Nor or DocumentQueryOperationType.Or;
                            if (!isElementMatches && !isLogicalOperation && string.IsNullOrEmpty(eachQuery.Field))
                                throw new ArgumentException(
                                    "Field must be specified for all operations besides logical operations and operations in element matches.");

                            var innerOperationValue = new Dictionary<string, object>
                            {
                                {
                                    GetStringOperationType(eachQuery.OperationType),
                                    ResolveOperationValue(eachQuery.OperationType, eachQuery.Value)
                                }
                            };

                            if (string.IsNullOrEmpty(eachQuery.Field)) return innerOperationValue;

                            return new Dictionary<string, object>
                            {
                                {
                                    eachQuery.Field,
                                    innerOperationValue
                                }
                            };
                        });

                        break;
                    }
                }

                return operationValue;
            }

            var queryRequestData = new Dictionary<string, object>();

            // Resolve each operation in the query to a format acceptable by the server
            foreach (var eachQuery in queries)
            {
                var field = eachQuery.Field;
                var operationType = eachQuery.OperationType;
                var isLogicalOperation = operationType is DocumentQueryOperationType.And
                    or DocumentQueryOperationType.Nor or DocumentQueryOperationType.Or;
                var operationKeyStr = GetStringOperationType(operationType);
                var operationValue = ResolveOperationValue(operationType, eachQuery.Value);

                if ((isElementMatches || isLogicalOperation) && string.IsNullOrEmpty(field))
                {
                    queryRequestData[operationKeyStr] = operationValue;
                    continue;
                }

                if (string.IsNullOrEmpty(field))
                    throw new ArgumentException(
                        "Field must be specified for all operations besides logical operations and operations in element matches.");

                queryRequestData[field] = new Dictionary<string, object>
                {
                    { GetStringOperationType(operationType), operationValue }
                };
            }

            return queryRequestData;
        }
    }

    public enum DocumentQueryOperationType
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        In,
        NotIn,
        Exists,
        DataTypeIs,
        Modulo,
        MatchesRegex,
        GeoIntersects,
        GeoWithin,
        GeoNear,
        ContainsAllOf,
        ElementMatches,
        ArraySizeIs,
        And,
        Nor,
        Or
    }
}