using System;
using System.Collections.Generic;
using System.Reflection;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet.ObjectDescriptors
{
    internal class PrimitiveTypesObjectDescriptorProvider : IObjectDescriptorProvider
    {
        private static readonly PrimitiveTypeValueAccessor PrimitiveValueAccessor = new PrimitiveTypeValueAccessor();
        private static readonly Func<ObjectDescriptor, object, IEnumerable<object>> GetProperties = (descriptor, o) => ArrayEx.Empty<string>();
        private static readonly Type StringType = typeof(string);
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!(type.GetTypeInfo().IsPrimitive || StringType == type))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            value = new ObjectDescriptor(type, PrimitiveValueAccessor, GetProperties);
            return true;
        }
        
        private class PrimitiveTypeValueAccessor : IMemberAccessor
        {
            public bool TryGetValue(object instance, Type instanceType, ChainSegment memberName, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}