using System;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;
using Newtonsoft.Json.Linq;

namespace HandlebarsDotNet.Extension.NewtonsoftJson
{
    internal class JArrayMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object? value)
        {
            if (int.TryParse(memberName.TrimmedValue, out var index))
            {
                var jArray = (JArray) instance;
                value = jArray[index];
                return true;
            }

            value = null;
            return false;
        }
    }
}