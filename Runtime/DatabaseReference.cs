using System.Linq;

namespace SphereKit
{
    public class DatabaseReference
    {
        public string Path { get; private set; }
        public string Id { get => Path.Split("/").Last(); }

        public DatabaseReference(string path)
        {
            Path = path;
        }

    }
}
