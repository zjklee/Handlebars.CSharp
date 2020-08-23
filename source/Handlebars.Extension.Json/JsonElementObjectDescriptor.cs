using System;
using System.Collections;
using System.Text.Json;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet.Extension.Json
{
    internal class JsonElementObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(JsonElement);
        
        private readonly ObjectDescriptor _descriptor = new ObjectDescriptor(
            Type, new JsonElementMemberAccessor(), GetEnumerator
        );
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (Type != type)
            {
                value = ObjectDescriptor.Empty;
                return false;
            }

            value = _descriptor;
            return true;
        }
        
        private static IEnumerable GetEnumerator(ObjectDescriptor descriptor, object instance)
        {
            var document = (JsonElement) instance;
            return Utils.GetEnumerator(document);
        }
    }
}