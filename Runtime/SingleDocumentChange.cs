#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace SphereKit
{
    [Preserve]
    [DataContract]
    public class SingleDocumentChange
    {
        public SingleDocumentChangeType Type { get; private set; }
        public Document Document { get; private set; }

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
                        Type = SingleDocumentChangeType.Initial;
                        break;
                    case "update":
                        Type = SingleDocumentChangeType.Update;
                        break;
                    default:
                        throw new Exception("Invalid change operation type received: " + value);
                }
            }
        }

        [Preserve] [DataMember(IsRequired = true, Name = "document")]
        private Dictionary<string, object>? _document;

        internal void GenerateDocument(DocumentReference reference)
        {
            if (_document != null) Document = new Document(reference, _document);
        }
    }

    public enum SingleDocumentChangeType
    {
        Initial,
        Update
    }
}