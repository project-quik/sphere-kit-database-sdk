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
        public MultiDocumentChangeType Type { get; private set; }
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
                        Type = MultiDocumentChangeType.Initial;
                        break;
                    case "update":
                        Type = MultiDocumentChangeType.Update;
                        break;
                    case "delete":
                        Type = MultiDocumentChangeType.Delete;
                        break;
                    default:
                        throw new Exception("Invalid change operation type received: " + value);
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

    public enum MultiDocumentChangeType
    {
        Initial,
        Update,
        Delete
    }
}