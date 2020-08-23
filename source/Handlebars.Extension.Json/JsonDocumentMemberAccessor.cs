using System;
using System.Text.Json;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.Extension.Json
{
    internal class JsonDocumentMemberAccessor : IMemberAccessor
    {
        private readonly JsonElementMemberAccessor _jsonElementMemberAccessor = new JsonElementMemberAccessor();
        
        public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object? value)
        {
            var document = (JsonDocument) instance;
            return _jsonElementMemberAccessor.TryGetValue(document.RootElement, instanceType, memberName, out value);
        }
    }
}