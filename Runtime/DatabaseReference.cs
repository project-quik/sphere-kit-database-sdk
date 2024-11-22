using System.Linq;

namespace SphereKit
{
    public abstract class DatabaseReference
    {
        public string Path { get; }
        public string Id => Path.Split("/").Last();
        public Database Database { get; }

        protected DatabaseReference(string path, Database database)
        {
            Path = path;
            Database = database;
        }
    }
}
