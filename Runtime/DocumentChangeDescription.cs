using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace SphereKit
{
    [Preserve]
    [DataContract]
    public class DocumentChangeDescription
    {
        [Preserve] [DataMember(IsRequired = true, Name = "updatedFields")]
        public readonly Dictionary<string, object> UpdatedFields = new();

        [Preserve] [DataMember(IsRequired = true, Name = "removedFields")]
        public readonly string[] DeletedFields = Array.Empty<string>();
    }
}