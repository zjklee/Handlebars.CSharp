using System.Collections.Generic;

namespace HandlebarsDotNet.MemberAccessors.EnumerableAccessors
{
    internal sealed class StructListMemberAccessor<T, TV> : EnumerableMemberAccessor
        where T: IList<TV>
        where TV: struct
    {
        protected override bool TryGetValueInternal(object instance, int index, out object value)
        {
            var list = (T) instance;
            value = list[index];
            return true;
        }
    }
}