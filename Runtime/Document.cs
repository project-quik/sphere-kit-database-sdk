using System.Collections.Generic;

namespace SphereKit
{
    public class Document
    {
        public DocumentReference Reference { get; }
        public string Id => Reference.Id;
        public Dictionary<string, object> Data { get; }

        internal Document() { }
        
        internal Document(DocumentReference reference, Dictionary<string, object> data) {
            Reference = reference;
            Data = data;
        }
    }
}
