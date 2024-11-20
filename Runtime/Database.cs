#nullable enable
namespace SphereKit
{
    public class Database
    {
        public static CollectionReference Collection(string path)
        {
            return new CollectionReference(path);
        }
        
        public static DocumentReference Document(string path)
        {
            return new DocumentReference(path);
        }
    }
}
