using System;
using System.Collections.Generic;

#nullable enable
namespace SphereKit
{
    /// <summary>
    /// Represents a query on a document.
    /// </summary>
    public class DocumentQueryOperation
    {
        /// <summary>
        /// The type of query.
        /// </summary>
        public readonly DocumentQueryOperationType OperationType;

        /// <summary>
        /// The field being queried.
        /// </summary>
        public readonly string? Field;

        /// <summary>
        /// The value to query on the field.
        /// </summary>
        public readonly object Value;

        private DocumentQueryOperation(DocumentQueryOperationType operationType, string? field, object value)
        {
            OperationType = operationType;
            Field = field;
            Value = value;
        }

        /// <summary>
        /// Checks if a value is a number (long/int/float/double).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <exception cref="ArgumentException">The value is not a number.</exception>
        private static void CheckNumberValue(object value)
        {
            if (value is not long && value is not int && value is not float && value is not double)
                throw new ArgumentException("Value must be a number (long/int/float/double).");
        }

        /// <summary>
        /// Checks if a value is an array or a list.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <exception cref="ArgumentException">The value is not an array or a list.</exception>
        private static void CheckArrayValue(object value)
        {
            if (value is not Array && value is not List<object>)
                throw new ArgumentException("Value must be an array or list.");
        }

        /// <summary>
        /// Require all queries in the array of queries to be true to succeed.
        /// </summary>
        /// <param name="queries">The queries to evaluate.</param>
        /// <returns></returns>
        public static DocumentQueryOperation And(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.And, null, queries);
        }

        /// <summary>
        /// Require none of the queries in the array of queries to be true to succeed.
        /// </summary>
        /// <param name="queries">The queries to evaluate.</param>
        /// <returns></returns>
        public static DocumentQueryOperation Nor(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Nor, null, queries);
        }

        /// <summary>
        /// Require one of the queries in the array of queries to be true to succeed.
        /// </summary>
        /// <param name="queries">The queries to evaluate.</param>
        /// <returns></returns>
        public static DocumentQueryOperation Or(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Or, null, queries);
        }

        /// <summary>
        /// Checks if the field value is equal to the specified value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentQueryOperation Equal(string? field, object value)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Equal, field, value);
        }

        /// <summary>
        /// Checks if the field value is not equal to the specified value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentQueryOperation NotEqual(string? field, object value)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.NotEqual, field, value);
        }

        /// <summary>
        /// Checks if the field value is greater than the specified value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentQueryOperation GreaterThan(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.GreaterThan, field, value);
        }

        /// <summary>
        /// Checks if the field value is greater than or equal to the specified value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentQueryOperation GreaterThanOrEqual(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.GreaterThanOrEqual, field, value);
        }

        /// <summary>
        /// Checks if the field value is less than the specified value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentQueryOperation LessThan(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.LessThan, field, value);
        }

        /// <summary>
        /// Checks if the field value is less than or equal to the specified value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentQueryOperation LessThanOrEqual(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckNumberValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.LessThanOrEqual, field, value);
        }

        /// <summary>
        /// Checks if the value is present in the array field.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public static DocumentQueryOperation In(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckArrayValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.In, field, value);
        }

        /// <summary>
        /// Checks if the value is not present in the array field.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public static DocumentQueryOperation NotIn(string? field, object value)
        {
            if (value is not DocumentQueryProjection) CheckArrayValue(value);

            return new DocumentQueryOperation(DocumentQueryOperationType.NotIn, field, value);
        }

        /// <summary>
        /// Checks if the field is set.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <returns></returns>
        public static DocumentQueryOperation Exists(string? field)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Exists, field, true);
        }

        /// <summary>
        /// Checks if the field is not set.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <returns></returns>
        public static DocumentQueryOperation NotExists(string? field)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Exists, field, false);
        }

        /// <summary>
        /// Checks if the field data type is a certain data type.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="type">The data type to check.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks if the remainder of the field value divided by a divisor is equal to a certain value.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="remainder">The expected remainder.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Cannot divide by zero.</exception>
        public static DocumentQueryOperation Modulo(string? field, object divisor, object remainder)
        {
            CheckNumberValue(divisor);
            if ((int)divisor == 0)
                throw new ArgumentException("Cannot divide by zero.");

            CheckNumberValue(remainder);

            return new DocumentQueryOperation(DocumentQueryOperationType.Modulo, field, new[] { divisor, remainder });
        }

        /// <summary>
        /// Checks if the field value matches a regular expression pattern.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns></returns>
        public static DocumentQueryOperation MatchesRegex(string? field, string pattern)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.MatchesRegex, field, pattern);
        }

        /// <summary>
        /// Checks if the GeoJSON field intersects this GeoJSON object.<br></br>
        /// Supported GeoJSON types are: Point, MultiPoint, LineString, MultiLineString, Polygon, MultiPolygon, GeometryCollection
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="geometry">The GeoJSON object.</param>
        /// <returns></returns>
        public static DocumentQueryOperation GeoIntersects(string? field, object geometry)
        {
            GeoJsonValidator.Validate(geometry);

            return new DocumentQueryOperation(DocumentQueryOperationType.GeoIntersects, field,
                new Dictionary<string, object>
                {
                    { "$geometry", geometry }
                });
        }

        /// <summary>
        /// Checks if the GeoJSON field is within this GeoJSON object.<br></br>
        /// Supported GeoJSON types are: Polygon, MultiPolygon
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="geometry">The GeoJSON object.</param>
        /// <returns></returns>
        public static DocumentQueryOperation GeoWithin(string? field, object geometry)
        {
            GeoJsonValidator.Validate(geometry, new[] { "Polygon", "MultiPolygon" });

            return new DocumentQueryOperation(DocumentQueryOperationType.GeoWithin, field,
                new Dictionary<string, object>
                {
                    { "$geometry", geometry }
                });
        }

        /// <summary>
        /// Checks if the GeoJSON field is within a specified distance range from this GeoJSON object.<br></br>
        /// Supported GeoJSON types are: Point
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="geometry">The GeoJSON object.</param>
        /// <param name="minDistance">The minimum distance the GeoJSON field can be from this GeoJSON object.</param>
        /// <param name="maxDistance">The maximum distance the GeoJSON field can be from this GeoJSON object.</param>
        /// <returns></returns>
        public static DocumentQueryOperation GeoNear(string? field, object geometry, long? minDistance = null,
            long? maxDistance = null)
        {
            GeoJsonValidator.Validate(geometry, new[] { "Point" });

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

        /// <summary>
        /// Check that the array field contains all the values specified.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="values">The values to check.</param>
        /// <returns></returns>
        public static DocumentQueryOperation ContainsAllOf(string? field, object values)
        {
            CheckArrayValue(values);

            return new DocumentQueryOperation(DocumentQueryOperationType.ContainsAllOf, field, values);
        }

        /// <summary>
        /// Check that each element in the array field matches an array of subqueries.<br></br>
        /// Specify null for the field parameter for subqueries where you would like to refer to the element being checked, and the subpath if you would like to check the inner fields of the element.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="queries">The queries on each element.</param>
        /// <returns></returns>
        public static DocumentQueryOperation ElementMatches(string field, DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.ElementMatches, field, queries);
        }

        /// <summary>
        /// Checks if the array field has a certain number of elements.
        /// </summary>
        /// <param name="field">The field path.</param>
        /// <param name="size">The expected number of elements.</param>
        /// <returns></returns>
        public static DocumentQueryOperation ArraySizeIs(string? field, long size)
        {
            CheckNumberValue(size);

            return new DocumentQueryOperation(DocumentQueryOperationType.ArraySizeIs, field, size);
        }

        /// <summary>
        /// Gets the string representation of the operation type.
        /// </summary>
        /// <param name="operationType">The operation type.</param>
        /// <returns>The string representation.</returns>
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