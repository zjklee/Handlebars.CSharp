using System;
using System.Reflection;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet.ObjectDescriptors
{
    internal sealed class RefObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(Ref);
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!Type.IsAssignableFrom(type))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            var accessor = new RefMemberAccessor();
            value = new ObjectDescriptor(type, accessor, (descriptor, o) => ArrayEx.Empty<string>());
            return true;
        }
    }
}