using System.Collections.Generic;
using System.Linq;

namespace HandlebarsDotNet.MemberAccessors.EnumerableAccessors
{
    internal sealed class StructEnumerableMemberAccessor<T, TV> : EnumerableMemberAccessor
        where T: class, IEnumerable<TV>
        where TV: struct
    {
        protected override bool TryGetValueInternal(object instance, int index, out object value)
        {
            var list = (T) instance;
            value = list.ElementAtOrDefault(index);
            return true;
        }
    }
}