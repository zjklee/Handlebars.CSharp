using System;
using System.Collections;
using System.Text.Json;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet.Extension.Json
{
    internal class JsonElementRefObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(Ref<JsonElement>);
        
        private readonly ObjectDescriptor _descriptor = new ObjectDescriptor(
            Type, new JsonElementRefMemberAccessor(), GetEnumerator
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
            var @ref = (Ref<JsonElement>) instance;
            return Utils.GetEnumerator(@ref.Value);
        }
    }
}