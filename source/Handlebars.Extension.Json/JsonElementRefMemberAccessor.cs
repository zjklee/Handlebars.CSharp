using System;
using System.Text.Json;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet.Extension.Json
{
    internal class JsonElementRefMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object? value)
        {
            var @ref = (Ref<JsonElement>) instance; 
            return Utils.TryGetValue(@ref.Value, memberName, out value);
        }
    }
}