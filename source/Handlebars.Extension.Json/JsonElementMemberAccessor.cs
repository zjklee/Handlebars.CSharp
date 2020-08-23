using System;
using System.Text.Json;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.Extension.Json
{
    internal class JsonElementMemberAccessor : IMemberAccessor
    {
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object? value)
        {
            var document = (JsonElement) instance;

            return Utils.TryGetValue(document, memberName, out value);
        }
    }
}