using System;
using System.Text.Json;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.Extension.Json
{
    internal class JsonElementReusableRefMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object? value)
        {
            var @ref = (ReusableRef<JsonElement>) instance; 
            return Utils.TryGetValue(@ref.Value, memberName, out value);
        }
    }
}