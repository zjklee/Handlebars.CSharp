using System;
using System.Collections.Generic;
using HandlebarsDotNet.ObjectDescriptors;
using Newtonsoft.Json.Linq;

namespace HandlebarsDotNet.Extension.NewtonsoftJson
{
    internal class JObjectDescriptorProvider : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(JObject);
        private static readonly JObjectMemberAccessor MemberAccessor = new JObjectMemberAccessor();
        private static readonly ObjectDescriptor ObjectDescriptor = 
            new ObjectDescriptor(typeof(JObject), MemberAccessor, (descriptor, o) => ((IDictionary<string, JToken>) o).Keys);
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (Type != type)
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            value = ObjectDescriptor;
            return true;
        }
    }
}