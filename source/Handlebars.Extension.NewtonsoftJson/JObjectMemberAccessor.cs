using System;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;
using Newtonsoft.Json.Linq;

namespace HandlebarsDotNet.Extension.NewtonsoftJson
{
    internal class JObjectMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object? value)
        {
            var jObject = (JObject) instance;
            if (jObject.TryGetValue(memberName, StringComparison.OrdinalIgnoreCase, out var token))
            {
                value = token is JValue jValue 
                    ? jValue.Value 
                    : token;

                return true;
            }

            value = null;
            return false;
        }
    }
}