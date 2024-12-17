using System.Collections.Generic;

namespace SphereKit
{
    /// <summary>
    /// Represents a logical group of documents.
    /// </summary>
    public class Collection
    {
        public CollectionReference Reference { get; }
        public string Id => Reference.Id;
        public Document[] Documents { get; }

        internal Collection()
        {
        }

        internal Collection(CollectionReference reference,
            Dictionary<string, Dictionary<string, object>> documentsResponse)
        {
            Reference = reference;

            var documents = new List<Document>();
            foreach (var (id, value) in documentsResponse)
                documents.Add(new Document(new DocumentReference($"{reference.Path}/{id}", reference.Database), value));

            Documents = documents.ToArray();
        }
    }
}