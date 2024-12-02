using System;
using System.Collections.Generic;
using Codice.CM.Common.Checkin.Partial.DifferencesApplier;

namespace SphereKit
{
    public class DocumentQueryOperation
    {
        public static string And => "$and";
        public static string Nor => "$nor";
        public static string Or => "$or";
        
        public readonly DocumentQueryOperationType OperationType;
        public readonly object Value;
        
        private DocumentQueryOperation(DocumentQueryOperationType operationType, object value)
        {
            OperationType = operationType;
            Value = value;
        }
        
        private static void CheckNumberValue(object value)
        {
            if (value is not long && value is not int && value is not float && value is not double)
            {
                throw new ArgumentException("Value must be a number (long/int/float/double).");
            }
        }
        
        private static void CheckArrayValue(object value)
        {
            if (value is not Array && value is not List<object>)
            {
                throw new ArgumentException("Value must be an array or list.");
            }
        }
        
        public static DocumentQueryOperation AndQuery(Dictionary<string, DocumentQueryOperation> queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.And, queries);
        }
        
        public static DocumentQueryOperation AndQuery(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.And, queries);
        }
        
        public static DocumentQueryOperation NorQuery(Dictionary<string, DocumentQueryOperation> queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Nor, queries);
        }
        
        public static DocumentQueryOperation NorQuery(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Nor, queries);
        }
        
        public static DocumentQueryOperation OrQuery(Dictionary<string, DocumentQueryOperation> queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Or, queries);
        }
        
        public static DocumentQueryOperation OrQuery(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Or, queries);
        }
        
        public static DocumentQueryOperation Equal(object value)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Equal, value);
        }
        
        public static DocumentQueryOperation NotEqual(object value)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.NotEqual, value);
        }
        
        public static DocumentQueryOperation GreaterThan(object value)
        {
            if (value is not DocumentQueryProjection)
            {
                CheckNumberValue(value);
            }
                
            return new DocumentQueryOperation(DocumentQueryOperationType.GreaterThan, value);
        }
        
        public static DocumentQueryOperation GreaterThanOrEqual(object value)
        {
            if (value is not DocumentQueryProjection)
            {
                CheckNumberValue(value);
            }
                
            return new DocumentQueryOperation(DocumentQueryOperationType.GreaterThanOrEqual, value);
        }
        
        public static DocumentQueryOperation LessThan(object value)
        {
            if (value is not DocumentQueryProjection)
            {
                CheckNumberValue(value);
            }
                
            return new DocumentQueryOperation(DocumentQueryOperationType.LessThan, value);
        }
        
        public static DocumentQueryOperation LessThanOrEqual(object value)
        {
            if (value is not DocumentQueryProjection)
            {
                CheckNumberValue(value);
            }
                
            return new DocumentQueryOperation(DocumentQueryOperationType.LessThanOrEqual, value);
        }
        
        public static DocumentQueryOperation In(object value)
        {
            if (value is not DocumentQueryProjection)
            {
                CheckArrayValue(value);
            }
            
            return new DocumentQueryOperation(DocumentQueryOperationType.In, value);
        }
        
        public static DocumentQueryOperation NotIn(object value)
        {
            if (value is not DocumentQueryProjection)
            {
                CheckArrayValue(value);
            }
            
            return new DocumentQueryOperation(DocumentQueryOperationType.NotIn, value);
        }
        
        public static DocumentQueryOperation Exists()
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Exists, true);
        }
        
        public static DocumentQueryOperation NotExists()
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.Exists, false);
        }
        
        public static DocumentQueryOperation DataTypeIs(SphereKitDataType type)
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
            
            return new DocumentQueryOperation(DocumentQueryOperationType.DataTypeIs, typeString);
        }
        
        public static DocumentQueryOperation Modulo(object divisor, object remainder)
        {
            CheckNumberValue(divisor);
            CheckNumberValue(remainder);
            
            return new DocumentQueryOperation(DocumentQueryOperationType.Modulo, new object[] {divisor, remainder});
        }
        
        public static DocumentQueryOperation MatchesRegex(string pattern)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.MatchesRegex, pattern);
        }
        
        public static DocumentQueryOperation GeoIntersects(object geometry)
        {
            GeoJsonValidator.Validate(geometry);
            
            return new DocumentQueryOperation(DocumentQueryOperationType.GeoIntersects, new Dictionary<string, object>
            {
                {"$geometry", geometry}
            });
        }
        
        public static DocumentQueryOperation GeoWithin(object geometry)
        {
            GeoJsonValidator.Validate(geometry);
            
            return new DocumentQueryOperation(DocumentQueryOperationType.GeoWithin, new Dictionary<string, object>
            {
                {"$geometry", geometry}
            });
        }
        
        public static DocumentQueryOperation GeoNear(object geometry, long? minDistance = null, long? maxDistance = null)
        {
            GeoJsonValidator.Validate(geometry);
            
            var operatorValue = new Dictionary<string, object>
            {
                { "$geometry", geometry },
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
            
            return new DocumentQueryOperation(DocumentQueryOperationType.GeoNear, operatorValue);
        }
        
        public static DocumentQueryOperation ContainsAllOf(object values)
        {
            CheckArrayValue(values);
            
            return new DocumentQueryOperation(DocumentQueryOperationType.ContainsAllOf, values);
        }
        
        public static DocumentQueryOperation ElementMatches(DocumentQueryOperation[] queries)
        {
            return new DocumentQueryOperation(DocumentQueryOperationType.ElementMatches, queries);
        }
        
        public static DocumentQueryOperation ArraySizeIs(long size)
        {
            CheckNumberValue(size);
            
            return new DocumentQueryOperation(DocumentQueryOperationType.ArraySizeIs, size);
        }

        public static string GetStringOperationType(DocumentQueryOperationType operationType)
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