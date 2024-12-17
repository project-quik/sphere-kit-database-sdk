using System;
using System.Collections.Generic;

#nullable enable
namespace SphereKit
{
    /// <summary>
    /// An update operation on a document field.
    /// </summary>
    public class DocumentDataOperation
    {
        /// <summary>
        /// The type of operation.
        /// </summary>
        public readonly DocumentDataOperationType OperationType;

        /// <summary>
        /// The value to apply to the field during the operation.
        /// </summary>
        public readonly object? Value;

        private DocumentDataOperation(DocumentDataOperationType operationType, object? value)
        {
            OperationType = operationType;
            Value = value;
        }

        /// <summary>
        /// Sets the field to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="onInsert">Whether to only set the field if the document does not exist.</param>
        /// <returns></returns>
        public static DocumentDataOperation Set(object value, bool onInsert = false)
        {
            return onInsert
                ? new DocumentDataOperation(DocumentDataOperationType.SetOnInsert, value)
                : new DocumentDataOperation(DocumentDataOperationType.Set, value);
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
        /// Increments the field by the specified value.
        /// </summary>
        /// <param name="value">The value to increment by.</param>
        /// <returns></returns>
        public static DocumentDataOperation Inc(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Inc, value);
        }

        /// <summary>
        /// Decrements the field by the specified value.
        /// </summary>
        /// <param name="value">The value to decrement by.</param>
        /// <returns></returns>
        public static DocumentDataOperation Dec(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Dec, value);
        }

        /// <summary>
        /// Sets the field to the minimum of the current value and the specified value.
        /// </summary>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentDataOperation Min(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Min, value);
        }

        /// <summary>
        /// Sets the field to the maximum of the current value and the specified value.
        /// </summary>
        /// <param name="value">The value to compare with.</param>
        /// <returns></returns>
        public static DocumentDataOperation Max(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Max, value);
        }

        /// <summary>
        /// Multiplies the field by the specified value.
        /// </summary>
        /// <param name="value">The value to multiply by.</param>
        /// <returns></returns>
        public static DocumentDataOperation Mul(object value)
        {
            CheckNumberValue(value);

            return new DocumentDataOperation(DocumentDataOperationType.Mul, value);
        }

        /// <summary>
        /// Divides the field by the specified value.
        /// </summary>
        /// <param name="value">The value to divide by.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Cannot divide by zero.</exception>
        public static DocumentDataOperation Div(object value)
        {
            CheckNumberValue(value);
            if ((int)value == 0)
                throw new ArgumentException("Cannot divide by zero.");

            return new DocumentDataOperation(DocumentDataOperationType.Div, value);
        }

        /// <summary>
        /// Renames the field or moves the field.
        /// The new field path can be a subfield (e.g. myobjectfield.subfield, myarrayfield.0)
        /// </summary>
        /// <param name="newPath">The new field path.</param>
        /// <returns></returns>
        public static DocumentDataOperation Rename(string newPath)
        {
            return new DocumentDataOperation(DocumentDataOperationType.Rename, newPath);
        }

        /// <summary>
        /// Unsets the field (removes the field and its data).
        /// </summary>
        /// <returns></returns>
        public static DocumentDataOperation Unset()
        {
            return new DocumentDataOperation(DocumentDataOperationType.Unset, null);
        }

        /// <summary>
        /// Adds an element to the array field if the element does not already exist.
        /// </summary>
        /// <param name="value">The element to add.</param>
        /// <returns></returns>
        public static DocumentDataOperation AddIfNotExists(object value)
        {
            return new DocumentDataOperation(DocumentDataOperationType.AddToSet, value);
        }

        /// <summary>
        /// Adds one or more elements to the array field if they do not already exist.
        /// </summary>
        /// <param name="values">The elements to add to the field.</param>
        /// <returns></returns>
        public static DocumentDataOperation AddManyIfNotExists(object[] values)
        {
            return new DocumentDataOperation(DocumentDataOperationType.AddToSet, new Dictionary<string, object>
            {
                { "$each", values }
            });
        }

        /// <summary>
        /// Removes the first element in the array field.
        /// </summary>
        /// <returns></returns>
        public static DocumentDataOperation RemoveFirst()
        {
            return new DocumentDataOperation(DocumentDataOperationType.Pop, -1);
        }

        /// <summary>
        /// Removes the last element in the array field.
        /// </summary>
        /// <returns></returns>
        public static DocumentDataOperation RemoveLast()
        {
            return new DocumentDataOperation(DocumentDataOperationType.Pop, 1);
        }

        /// <summary>
        /// Adds an element to the array field.
        /// </summary>
        /// <param name="value">The element to add to the field.</param>
        /// <returns></returns>
        public static DocumentDataOperation Add(object value)
        {
            return new DocumentDataOperation(DocumentDataOperationType.Push, value);
        }

        /// <summary>
        /// Adds one or more elements to the array field.
        /// </summary>
        /// <param name="values">The elements to add to the field.</param>
        /// <param name="position">The position at which the elements should be inserted at.</param>
        /// <param name="slice">The number of elements in the array to keep.</param>
        /// <param name="sort">The sort specification for the array. Either a <see cref="FieldSortDirection"/> to sort primitive elements, or a dictionary with one key-value pair (with element subfield key as key and <see cref="FieldSortDirection"/> as value).</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static DocumentDataOperation AddMany(object[] values, int? position = null, ArraySlice? slice = null,
            object? sort = null)
        {
            var operationData = new Dictionary<string, object>
            {
                { "$each", values }
            };
            if (position != null) operationData["$position"] = position;
            if (slice != null) operationData["$slice"] = slice.Value.Slice;
            if (sort != null)
            {
                if (sort is not FieldSortDirection && sort is not Dictionary<string, FieldSortDirection>)
                    throw new ArgumentException(
                        "Sort must be a FieldSortDirection or a dictionary with one key-value pair (key: string, value: FieldSortDireection).");

                operationData["$sort"] = sort;
            }

            return new DocumentDataOperation(DocumentDataOperationType.Push, operationData);
        }

        /// <summary>
        /// Gets the string representation of the operation type.
        /// </summary>
        /// <param name="operationType">The operation type.</param>
        /// <returns>The string representation.</returns>
        private static string GetStringOperationType(DocumentDataOperationType operationType)
        {
            return operationType switch
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
        }

        /// <summary>
        /// This method converts a dictionary of update operations into a dictionary of request data that can be sent to the server.
        /// </summary>
        /// <param name="update">The update specification, with field as key and operation as value.</param>
        /// <returns>The request data.</returns>
        internal static Dictionary<string, object> ConvertUpdateToRequestData(
            Dictionary<string, DocumentDataOperation> update)
        {
            var updateRequestData = new Dictionary<string, object>();
            foreach (var (fieldKey, value) in update)
            {
                var operationKey = value.OperationType;
                var operationValue = value.Value;
                var operationKeyStr = GetStringOperationType(operationKey);

                // Unset operation fields will be sent as a list of field keys.
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
                // Other operation fields will be sent as a dictionary of field and values.
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

    public struct ArraySlice
    {
        internal readonly int Slice;

        private ArraySlice(int slice)
        {
            Slice = slice;
        }

        public static ArraySlice First(int numberOfElements)
        {
            return new ArraySlice(numberOfElements);
        }

        public static ArraySlice Last(int numberOfElements)
        {
            return new ArraySlice(-numberOfElements);
        }
    }
}