using System;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet;

namespace HandlebarsDotNet.ObjectDescriptors
{
    public class TypeDescriptor
    {
        private static readonly Func<ValuePair<Type, ICompiledHandlebarsConfiguration>, TypeDescriptor> ValueFactory = 
            deps => new TypeDescriptor(deps.Item1, deps.Item2);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeDescriptor Create(object value, ICompiledHandlebarsConfiguration configuration)
        {
            if (ReferenceEquals(value, null)) return null;
            return Create(value.GetType(), configuration);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeDescriptor Create(Type type, ICompiledHandlebarsConfiguration configuration)
        {
            var typeDescriptors = configuration.TemplateContext.TypeDescriptors;
            if (typeDescriptors.TryGetValue(type, out var entry))
            {
                return entry.Value;
            }

            return typeDescriptors.GetOrAdd(type, (t, c) =>
            {
                var state = new ValuePair<Type, ICompiledHandlebarsConfiguration>(t, c);
                return new DeferredValue<ValuePair<Type, ICompiledHandlebarsConfiguration>, TypeDescriptor>(state, ValueFactory);
            }, configuration).Value;
        }

        public readonly Type Type;
        private readonly ICompiledHandlebarsConfiguration _configuration;

        private TypeDescriptor(Type type, ICompiledHandlebarsConfiguration configuration)
        {
            Type = type;
            _configuration = configuration;
            _wellKnownObjectDescriptor = GetWellKnownDescriptor(type, configuration);
        }

        private ObjectDescriptor _wellKnownObjectDescriptor;

        public bool TryGetObjectDescriptor(out ObjectDescriptor descriptor)
        {
            if (!ReferenceEquals(_wellKnownObjectDescriptor, null))
            {
                descriptor = _wellKnownObjectDescriptor;
                return true;
            }

            if (_configuration.ObjectDescriptorProvider.TryGetDescriptor(Type, out descriptor))
            {
                _wellKnownObjectDescriptor = descriptor;
                return true;
            }
            
            return false;
        }

        private static ObjectDescriptor GetWellKnownDescriptor(Type type, ICompiledHandlebarsConfiguration configuration)
        {
            return configuration.ObjectDescriptorProvider.TryGetDescriptor(type, out var descriptor)
                ? descriptor
                : null;
        }
    }
}