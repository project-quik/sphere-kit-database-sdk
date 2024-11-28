using System;

#nullable enable
namespace SphereKit
{
    public class DocumentQueryProjection
    {
        public string Field;
        
        public DocumentQueryProjection(string field)
        {
            Field = field;
        }
    }
}