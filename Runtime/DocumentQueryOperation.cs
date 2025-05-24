using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;

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
            if (value is not DocumentQueryExpression) CheckNumberValue(value);

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
            if (value is not DocumentQueryExpression) CheckNumberValue(value);

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
            if (value is not DocumentQueryExpression) CheckNumberValue(value);

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
            if (value is not DocumentQueryExpression) CheckNumberValue(value);

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
            if (value is not DocumentQueryExpression) CheckArrayValue(value);

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
            if (value is not DocumentQueryExpression) CheckArrayValue(value);

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
            if (divisor is not DocumentQueryExpression)
            {
                CheckNumberValue(divisor);
                if ((int)divisor == 0)
                    throw new ArgumentException("Cannot divide by zero.");
            }

            if (remainder is not DocumentQueryExpression) CheckNumberValue(remainder);

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
            if (geometry is not DocumentQueryExpression) GeoJsonValidator.Validate(geometry);

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
            if (geometry is not DocumentQueryExpression)
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
        public static DocumentQueryOperation GeoNear(string? field, object geometry, object? minDistance = null,
            object? maxDistance = null)
        {
            if (geometry is not DocumentQueryExpression) GeoJsonValidator.Validate(geometry, new[] { "Point" });

            var operatorValue = new Dictionary<string, object>
            {
                { "$geometry", geometry }
            };

            if (minDistance != null)
            {
                if (minDistance is not DocumentQueryExpression) CheckNumberValue(minDistance);
                operatorValue.Add("$minDistance", minDistance);
            }

            if (maxDistance != null)
            {
                if (maxDistance is not DocumentQueryExpression) CheckNumberValue(maxDistance);
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
            if (values is not DocumentQueryExpression) CheckArrayValue(values);

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
        public static DocumentQueryOperation ArraySizeIs(string? field, object size)
        {
            if (size is not DocumentQueryExpression) CheckNumberValue(size);

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
                DocumentQueryOperationType.GeoNear => "$nearSphere",
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
        /// Converts an array of DocumentQueryOperation objects to a JSON-friendly query structure (without expressions).
        /// This only works for operations with no expressions.
        /// </summary>
        /// <param name="queries">The array of queries to be converted.</param>
        /// <param name="isElementMatches">Whether the query is in an element match query.</param>
        /// <returns>Relevant JSON-friendly query data that can be used in the request body.</returns>
        private static Dictionary<string, object> ConvertQueryToRequestDataNoExpr(DocumentQueryOperation[] queries,
            bool isElementMatches = false)
        {
            // This method resolves the value of an operation in a query to a format acceptable by the server.
            //
            // Parameters:
            // - operationType: The type of the operation.
            // - operationValue: The value of the operation.
            //
            // Returns: The resolved value of the operation to be added to the request data.
            object ResolveOperationValue(DocumentQueryOperationType operationType, object operationValue)
            {
                if (isElementMatches && operationValue is DocumentQueryExpression)
                    throw new ArgumentException(
                        "Element matches cannot contain expressions.");

                if (operationValue is Array or List<object> &&
                    (operationValue as object[])!.All(item => item is DocumentQueryOperation))
                {
                    // Convert an array of operations
                    var queryList = (operationValue as DocumentQueryOperation[])!;

                    if (operationType == DocumentQueryOperationType.ElementMatches)
                        // For $elemMatch, recursively call convertQueryToRequestData with isElementMatches = true
                        return ConvertQueryToRequestDataNoExpr(queryList, true);

                    // For logical operators ($and, $or, $nor), convert each operation in the array
                    return queryList.Select(eachQuery =>
                    {
                        var isLogical = eachQuery.OperationType is DocumentQueryOperationType.And
                            or DocumentQueryOperationType.Nor or DocumentQueryOperationType.Or;

                        if (!isElementMatches && !isLogical && string.IsNullOrEmpty(eachQuery.Field))
                            throw new ArgumentException(
                                "Field must be specified for all operations besides logical operations and operations in element matches.");

                        var innerResolvedValue = ResolveOperationValue(eachQuery.OperationType, eachQuery.Value);
                        var innerOperation = new Dictionary<string, object>
                        {
                            { GetStringOperationType(eachQuery.OperationType), innerResolvedValue }
                        };

                        if (string.IsNullOrEmpty(eachQuery.Field))
                            return innerOperation;
                        return new Dictionary<string, object>
                        {
                            { eachQuery.Field, innerOperation }
                        };
                    });
                }

                // Return other values directly
                return operationValue;
            }

            var queryRequestData = new Dictionary<string, object>();

            foreach (var eachQuery in queries)
            {
                var field = eachQuery.Field;
                var operationType = eachQuery.OperationType;
                var isLogicalOperation = operationType is DocumentQueryOperationType.And
                    or DocumentQueryOperationType.Nor or DocumentQueryOperationType.Or;
                var operationKeyStr = GetStringOperationType(operationType);
                var resolvedValue = ResolveOperationValue(operationType, eachQuery.Value);

                if ((isElementMatches || isLogicalOperation) && string.IsNullOrEmpty(field))
                {
                    // Handle logical operators or operations directly within $elemMatch
                    queryRequestData[operationKeyStr] = resolvedValue;
                }
                else if (!string.IsNullOrEmpty(field))
                {
                    // Handle standard field operations { field: { $op: value } }
                    // If the field already exists (e.g., multiple conditions on the same field), merge the operations.
                    if (!queryRequestData.ContainsKey(field))
                        queryRequestData[field] = new Dictionary<string, object>();
                    (queryRequestData[field] as Dictionary<string, object>)![operationKeyStr] = resolvedValue;
                }
                else
                {
                    // Field is required for non-logical operations outside of $elemMatch
                    throw new ArgumentException(
                        "Field must be specified for all operations besides logical operations and operations in element matches.");
                }
            }

            return queryRequestData;
        }

        /// <summary>
        /// Converts an array of DocumentQueryOperation objects to a request body.
        /// </summary>
        /// <param name="queries">The array of queries to be converted.</param>
        /// <returns>The request body that can be used for getting or updating documents.</returns>
        internal static Dictionary<string, object> ConvertQueryToRequestData(DocumentQueryOperation[] queries)
        {
            // This method resolves the value of an operation in a query to a format acceptable by the server.
            //
            // Parameters:
            // - operationValue: The value of the operation.
            //
            // Returns: The resolved value of the operation to be added to the request data.
            object ResolveOperationValue(object operationValue)
            {
                if (operationValue is DocumentQueryExpression projection) return $"${projection.Field}";

                return operationValue;
            }

            // This method resolves a query to a format acceptable by the server.
            //
            // Parameters:
            // - query: The query to resolve.
            //
            // Returns: The resolved query as a dictionary.
            Dictionary<string, object> ResolveQuery(DocumentQueryOperation query)
            {
                var operationType = query.OperationType;
                var operationTypeStr = GetStringOperationType(operationType);
                var resolvedValue = ResolveOperationValue(query.Value);

                var isLogical = operationType is DocumentQueryOperationType.And
                    or DocumentQueryOperationType.Nor or DocumentQueryOperationType.Or;
                if (!isLogical && string.IsNullOrEmpty(query.Field))
                    throw new ArgumentException(
                        "Field must be specified for all operations besides logical operations and operations in element matches.");

                if (isLogical)
                    // For logical operators ($and, $or, $nor), convert each operation in the array
                    return new Dictionary<string, object>
                    {
                        {
                            operationTypeStr,
                            (query.Value as DocumentQueryOperation[])!.Select(ResolveQuery)
                        }
                    };

                if (operationType == DocumentQueryOperationType.ElementMatches)
                {
                    if (string.IsNullOrEmpty(query.Field))
                        throw new ArgumentException("Field must be specified for element matches operations.");

                    // For $elemMatch, recursively call convertQueryToRequestData without expressions with isElementMatches as true
                    return ConvertQueryToRequestDataNoExpr(new[] { query }, true);
                }

                // For standard field operations { field: { $op: value } }
                return new Dictionary<string, object>
                {
                    {
                        "$expr", new Dictionary<string, object>
                        {
                            { operationTypeStr, new[] { $"${query.Field}", resolvedValue } }
                        }
                    }
                };
            }

            if (queries.Length == 0) return new Dictionary<string, object>();

            var exprRequestData = queries.Select(ResolveQuery).ToList();

            return new Dictionary<string, object>
            {
                { "$and", exprRequestData }
            };
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