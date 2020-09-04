using System;
using System.Collections.Generic;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.MemberAccessors
{
    public sealed class KeyValuePairAccessor<T, TV> : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object value)
        {
            var keyValuePair = (KeyValuePair<T, TV>) instance;

            if (memberName.Equals("key"))
            {
                value = Ref.Create(keyValuePair.Key);
                return true;
            }
                
            if (memberName.Equals("value"))
            {
                value = Ref.Create(keyValuePair.Value);
                return true;
            }
                
            value = null;
            return false;
        }
    }
}