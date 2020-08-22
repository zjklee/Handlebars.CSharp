using System;
using System.Collections.Generic;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.MemberAccessors.DictionaryAccessors
{
    internal sealed class StringStructDictionaryAccessor<T, TV> : IMemberAccessor
        where T: IDictionary<string, TV>
        where TV: struct
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object value)
        {
            var dictionary = (T) instance;
            if (dictionary.TryGetValue(memberName.TrimmedValue, out var v))
            {
                value = v.AsRef();
                return true;
            }

            value = default(TV).AsRef();
            return false;
        }
    }
}