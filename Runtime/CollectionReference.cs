using System;

namespace SphereKit
{
    public class CollectionReference: DatabaseReference
    {
        public CollectionReference(string path): base(path)
        {
            if (path.Split("/").Length % 2 == 0)
            {
                throw new ArgumentException("CollectionReference path must have an odd number of segments.");
            }
        }

        public DocumentReference Document(string id)
        {
            return new DocumentReference($"{Path}/{id}");
        }
    }
}
