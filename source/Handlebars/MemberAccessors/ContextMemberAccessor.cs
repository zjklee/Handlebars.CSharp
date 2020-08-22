using System;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.MemberAccessors
{
    internal class ContextMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object value)
        {
            var bindingContext = (BindingContext) instance;
            return bindingContext.TryGetContextVariable(memberName, out value);
        }
    }
}