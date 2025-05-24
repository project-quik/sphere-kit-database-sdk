using System;

#nullable enable
namespace SphereKit
{
    public class DocumentQueryExpression
    {
        public string Field;

        public DocumentQueryExpression(string field)
        {
            Field = field;
        }
    }
}