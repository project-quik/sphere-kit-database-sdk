using System;
using System.Collections.Generic;

#nullable enable
namespace SphereKit
{
    public class DocumentDataOperation
    {
        public readonly DocumentDataOperationType OperationType;
        public readonly object? Value;

        private DocumentDataOperation(DocumentDataOperationType operationType, object? value)
        {
            OperationType = operationType;
            Value = value;
        }

        public static DocumentDataOperation Set(object value, bool onInsert = false)
        {
            return onInsert
                ? new DocumentDataOperation(DocumentDataOperationType.SetOnInsert, value)
                : new DocumentDataOperation(DocumentDataOperationType.Set, value);
        }

        private static void CheckNumberValue(object value)
        {
            if (value is not long && value is not int && value is not float && value is not double)
                throw new ArgumentException("Value must be a number (long/int/float/double).");
        }

        public static DocumentDataOperation Inc(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Inc, value);
        }

        public static DocumentDataOperation Dec(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Dec, value);
        }

        public static DocumentDataOperation Min(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Min, value);
        }

        public static DocumentDataOperation Max(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Max, value);
        }

        public static DocumentDataOperation Mul(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Mul, value);
        }

        public static DocumentDataOperation Div(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Div, value);
        }

        public static DocumentDataOperation Rename(string newPath)
        {
            return new DocumentDataOperation(DocumentDataOperationType.Rename, newPath);
        }

        public static DocumentDataOperation Unset()
        {
            return new DocumentDataOperation(DocumentDataOperationType.Unset, null);
        }

        public static DocumentDataOperation AddIfNotExists(object value)
        {
            return new DocumentDataOperation(DocumentDataOperationType.AddToSet, value);
        }

        public static DocumentDataOperation AddManyIfNotExists(object[] values)
        {
            return new DocumentDataOperation(DocumentDataOperationType.AddToSet, new Dictionary<string, object>
            {
                { "$each", values }
            });
        }

        public static DocumentDataOperation RemoveFirst()
        {
            return new DocumentDataOperation(DocumentDataOperationType.Pop, -1);
        }

        public static DocumentDataOperation RemoveLast()
        {
            return new DocumentDataOperation(DocumentDataOperationType.Pop, 1);
        }

        public static DocumentDataOperation Add(object value)
        {
            return new DocumentDataOperation(DocumentDataOperationType.Push, value);
        }

        public static DocumentDataOperation AddMany(object[] values, int? position = null, int? slice = null,
            object? sort = null)
        {
            var operationData = new Dictionary<string, object>
            {
                { "$each", values }
            };
            if (position != null) operationData["$position"] = position;
            if (slice != null) operationData["$slice"] = slice;
            if (sort != null)
            {
                if (sort is not int && sort is not Dictionary<string, int>)
                    throw new ArgumentException(
                        "Sort must be an integer (1 or -1) or a dictionary with one key-value pair (key: string, value: 1 | -1).");

                operationData["$sort"] = sort;
            }

            return new DocumentDataOperation(DocumentDataOperationType.Push, operationData);
        }

        internal static Dictionary<string, object> ConvertUpdateToRequestData(
            Dictionary<string, DocumentDataOperation> update)
        {
            var updateRequestData = new Dictionary<string, object>();
            foreach (var (fieldKey, value) in update)
            {
                var operationKey = value.OperationType;
                var operationValue = value.Value;
                var operationKeyStr = operationKey switch
                {
                    DocumentDataOperationType.Set => "$set",
                    DocumentDataOperationType.SetOnInsert => "$setOnInsert",
                    DocumentDataOperationType.Inc => "$inc",
                    DocumentDataOperationType.Dec => "$dec",
                    DocumentDataOperationType.Min => "$min",
                    DocumentDataOperationType.Max => "$max",
                    DocumentDataOperationType.Mul => "$mul",
                    DocumentDataOperationType.Div => "$div",
                    DocumentDataOperationType.Rename => "$rename",
                    DocumentDataOperationType.Unset => "$unset",
                    DocumentDataOperationType.AddToSet => "$addToSet",
                    DocumentDataOperationType.Pop => "$pop",
                    DocumentDataOperationType.Push => "$push",
                    _ => ""
                };

                if (operationKey != DocumentDataOperationType.Unset)
                {
                    if (!updateRequestData.TryGetValue(operationKeyStr, out var operationData))
                    {
                        operationData = new Dictionary<string, object>();
                        updateRequestData[operationKeyStr] = operationData;
                    }

                    var operationDataDict = (Dictionary<string, object>)operationData;
                    operationDataDict[fieldKey] = operationValue!;
                }
                else
                {
                    if (!updateRequestData.TryGetValue(operationKeyStr, out var operationData))
                    {
                        operationData = new List<object>();
                        updateRequestData[operationKeyStr] = operationData;
                    }

                    var operationDataList = (List<object>)operationData;
                    operationDataList.Add(fieldKey);
                }
            }

            return updateRequestData;
        }
    }

    public enum DocumentDataOperationType
    {
        Set,
        SetOnInsert,
        Inc,
        Dec,
        Min,
        Max,
        Mul,
        Div,
        Rename,
        Unset,
        AddToSet,
        Pop,
        Push
    }
}