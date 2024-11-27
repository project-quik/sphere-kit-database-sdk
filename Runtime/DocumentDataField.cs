#nullable enable
namespace SphereKit
{
    public class DocumentDataField
    {
        public readonly string FieldPath;
        
        private DocumentDataField(DocumentDataField parent, string key)
        {   
            FieldPath = $"{parent.FieldPath}.{key}";
        }

        public DocumentDataField(string baseField)
        {
            FieldPath = baseField;
        }

        public DocumentDataField Key(string key)
        {
            return new DocumentDataField(this, key);
        }
        
        public DocumentDataField ArrayItem(int index)
        {
            return new DocumentDataField(this, index.ToString());
        }

        public override string ToString()
        {
            return FieldPath;
        }
    }
}