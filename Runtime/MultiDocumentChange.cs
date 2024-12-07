#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace SphereKit
{
    [Preserve]
    [DataContract]
    public class MultiDocumentChange
    {
        public DocumentChangeType Type { get; private set; }
        public Document[] Documents { get; private set; } = Array.Empty<Document>();

        [Preserve] [DataMember(IsRequired = false, Name = "change")]
        public readonly DocumentChangeDescription? ChangeDescription;

        [Preserve]
        [DataMember(IsRequired = true, Name = "operationType")]
        private string _operationType
        {
            set
            {
                switch (value)
                {
                    case "initial":
                        Type = DocumentChangeType.Initial;
                        break;
                    case "update":
                        Type = DocumentChangeType.Update;
                        break;
                    case "delete":
                        Type = DocumentChangeType.Delete;
                        break;
                    default:
                        throw new Exception("Invalid operation type: " + value);
                }
            }
        }

        [Preserve] [DataMember(IsRequired = false, Name = "document")]
        private Dictionary<string, object>? _updatedDocument;

        [Preserve] [DataMember(IsRequired = false, Name = "name")]
        private string? _updatedDocumentId;

        [Preserve] [DataMember(IsRequired = false, Name = "documents")]
        private Dictionary<string, Dictionary<string, object>>? _initialDocuments;

        internal void GenerateDocuments(CollectionReference reference)
        {
            if (_updatedDocument != null)
            {
                Documents = new[] { new Document(reference.Document(_updatedDocumentId!), _updatedDocument) };
            }
            else if (_initialDocuments != null)
            {
                Documents = new Document[_initialDocuments.Count];
                var i = 0;
                foreach (var (id, value) in _initialDocuments)
                    Documents[i++] = new Document(reference.Document(id), value);
            }
        }
    }

    public enum DocumentChangeType
    {
        Initial,
        Update,
        Delete
    }

    [Preserve]
    [DataContract]
    public class DocumentChangeDescription
    {
        [Preserve] [DataMember(IsRequired = true, Name = "updatedFields")]
        public readonly Dictionary<string, object> UpdatedFields = new();

        [Preserve] [DataMember(IsRequired = true, Name = "removedFields")]
        public readonly string[] DeletedFields = Array.Empty<string>();
    }
}