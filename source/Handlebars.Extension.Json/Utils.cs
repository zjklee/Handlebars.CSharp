using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Extension.Json
{
    internal static class Utils
    {
        private static readonly Ref<bool> TrueRef = new Ref<bool>(true);
        private static readonly Ref<bool> FalseRef = new Ref<bool>(false);
        private static readonly Ref<object> NullRef = new Ref<object>(null!);
        
        public static IEnumerable GetEnumerator(JsonElement document)
        {
            return document.ValueKind switch
            {
                JsonValueKind.Object => EnumerateObject(),
                JsonValueKind.Array => EnumerateArray(),
                _ => throw new ArgumentOutOfRangeException()
            };

            IEnumerable<KeyValuePair<object, object>> EnumerateObject()
            {
                foreach (var property in document.EnumerateObject())
                {
                    yield return new KeyValuePair<object, object>(property.Name, ExtractProperty(property.Value));
                }
            }

            IEnumerable<object> EnumerateArray()
            {
                foreach (var property in document.EnumerateArray())
                {
                    yield return ExtractProperty(property);
                }
            }
        }

        private static object ExtractProperty(JsonElement property)
        {
            switch (property.ValueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return CreateRef(property);

                case JsonValueKind.String:
                    return property.GetString();

                case JsonValueKind.Number when property.TryGetInt64(out var @long):
                    return CreateRef(@long);

                case JsonValueKind.Number:
                    return CreateRef(property.GetDouble());

                case JsonValueKind.True:
                    return TrueRef;

                case JsonValueKind.False:
                    return FalseRef;

                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return NullRef;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool TryGetValue(JsonElement document, ChainSegment memberName, out object? value)
        {
            if (document.TryGetProperty(memberName.TrimmedValue, out var property))
            {
                switch (property.ValueKind)
                {
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        value = CreateRef(property);
                        return true;

                    case JsonValueKind.String:
                        value = property.GetString();
                        return true;

                    case JsonValueKind.Number when property.TryGetInt64(out var @long):
                        value = CreateRef(@long);
                        return true;

                    case JsonValueKind.Number:
                        value = CreateRef(property.GetDouble());
                        return true;

                    case JsonValueKind.True:
                        value = TrueRef;
                        return true;
                    
                    case JsonValueKind.False:
                        value = FalseRef;
                        return true;
                    
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                        value = null;
                        return false;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            value = null;
            return false;
        }
        
        private static ReusableRef<T> CreateRef<T>(T value)
        {
            return RefPool<T>.Shared.Create(value);
        }
    }
}