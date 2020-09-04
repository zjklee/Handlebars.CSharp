using System;
using System.Reflection;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet.ObjectDescriptors
{
    // TODO: should be considered for deletion
    internal sealed class RefObjectDescriptor : IObjectDescriptorProvider
    {
        private readonly ICompiledHandlebarsConfiguration _configuration;
        private static readonly Type Type = typeof(Ref);

        public RefObjectDescriptor(ICompiledHandlebarsConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!Type.IsAssignableFrom(type))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            var accessor = new RefMemberAccessor(_configuration);
            value = new ObjectDescriptor(type, accessor, (descriptor, o) => ArrayEx.Empty<string>());
            return true;
        }
    }
}