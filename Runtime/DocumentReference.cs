using System;

namespace SphereKit
{
    public class DocumentReference: DatabaseReference
    {
        public DocumentReference(string path): base(path)
        {
            if (path.Split("/").Length % 2 != 0)
            {
                throw new ArgumentException("DocumentReference path must have an even number of segments.");
            }
        }

        public CollectionReference Collection(string id)
        {
            return new CollectionReference($"{Path}/{id}");
        }
    }
}
