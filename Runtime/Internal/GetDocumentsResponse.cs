using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

#nullable enable
namespace SphereKit
{
    [Preserve]
    [DataContract]
    internal readonly struct GetDocumentsResponse
    {
        [Preserve] [DataMember(IsRequired = true, Name = "documents")]
        public readonly Dictionary<string, Dictionary<string, object>> Documents;
    }
}