using System.Collections.Generic;

namespace SphereKit
{
    public class Collection
    {
        public CollectionReference Reference { get; }
        public string Id => Reference.Id;
        public Document[] Documents { get; }

        internal Collection() { }
        
        internal Collection(CollectionReference reference, Dictionary<string, Dictionary<string, object>> documentsResponse) {
            Reference = reference;
            
            var documents = new List<Document>();
            foreach (var (path, value) in documentsResponse)
            {
                documents.Add(new Document(new DocumentReference(path, reference.Database), value));
            }
            Documents = documents.ToArray();
        }
    }
}
