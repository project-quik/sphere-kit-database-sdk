using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SphereKit
{
    public class DocumentReference: DatabaseReference
    {
        public DocumentReference(string path, Database database): base(path, database)
        {
            if (path.Length == 0) throw new ArgumentException("DocumentReference path must not be empty.");
            if (path.Split("/").Length % 2 != 0) throw new ArgumentException("DocumentReference path must have an even number of segments.");
        }

        public CollectionReference Collection(string id)
        {
            return new CollectionReference($"{Path}/{id}", Database);
        }
        
        public async Task<Document> Get()
        {
            return await Database.GetDocument(this);
        }
        
        public async Task Set(Dictionary<string, object> data)
        {
            await Database.SetDocument(this, data);
        }
        
        public async Task Update(Dictionary<string, DocumentDataOperation> data)
        {
            await Database.UpdateDocument(this, data);
        }

        public async Task Delete()
        {
            await Database.DeleteDocument(this);
        }
    }
}
