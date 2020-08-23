using System;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.MemberAccessors
{
    public class MergedMemberAccessor : IMemberAccessor
    {
        private readonly IMemberAccessor[] _accessors;

        public MergedMemberAccessor(params IMemberAccessor[] accessors)
        {
            _accessors = accessors;
        }

        public bool TryGetValue(object instance, Type type, ChainSegment memberName, out object value)
        {
            for (var index = 0; index < _accessors.Length; index++)
            {
                if (_accessors[index].TryGetValue(instance, type, memberName, out value)) return true;
            }

            value = default(object);
            return false;
        }
    }
}