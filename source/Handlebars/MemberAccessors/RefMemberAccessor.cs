using System;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet.MemberAccessors
{
    // TODO: should be considered for deletion
    internal sealed class RefMemberAccessor : IMemberAccessor
    {
        private readonly ICompiledHandlebarsConfiguration _configuration;

        public RefMemberAccessor(ICompiledHandlebarsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object value)
        {
            var @ref = (Ref) instance;
            var refValue = @ref.GetValue();
            if (TypeDescriptor.Create(refValue, _configuration).TryGetObjectDescriptor(out var descriptor))
            {
                return descriptor.MemberAccessor.TryGetValue(refValue, @ref.InnerType, memberName, out value);
            }
            
            value = null;
            return false;
        }
    }
}