using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

#nullable enable
namespace SphereKit
{
    public class CollectionReference: DatabaseReference
    {
        public CollectionReference(string path, Database database): base(path, database)
        {
            if (path.Length == 0) throw new ArgumentException("CollectionReference path must not be empty.");
            if (path.Split("/").Length % 2 == 0) throw new ArgumentException("CollectionReference path must have an odd number of segments.");
        }

        public DocumentReference Document(string id)
        {
            return new DocumentReference($"{Path}/{id}", Database);
        }

        public async Task Query(DocumentQueryOperation[]? query = null)
        { 
            await Database.QueryCollection(this, query);
        }
    }
}
