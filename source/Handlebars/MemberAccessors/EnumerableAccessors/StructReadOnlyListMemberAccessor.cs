using System.Collections.Generic;
using HandlebarsDotNet.Adapters;

namespace HandlebarsDotNet.MemberAccessors.EnumerableAccessors
{
    internal sealed class StructReadOnlyListMemberAccessor<T, TV> : EnumerableMemberAccessor
        where T: IReadOnlyList<TV>
        where TV: struct
    {
        protected override bool TryGetValueInternal(object instance, int index, out object value)
        {
            var list = (T) instance;
            value = list[index].AsRef();
            return true;
        }
    }
}