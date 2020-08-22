using System;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.MemberAccessors
{
    internal sealed class RefMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object value)
        {
            value = instance;
            return true;
        }
    }
}