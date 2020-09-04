using System;
using System.Collections;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;
using HandlebarsDotNet.Iterators;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.ObjectDescriptors
{
    public class ObjectDescriptorProvider : IObjectDescriptorProvider
    {
        private static readonly Type StringType = typeof(string);
        private readonly DynamicObjectDescriptor _dynamicObjectDescriptor;
        
        private readonly Type _dynamicMetaObjectProviderType = typeof(IDynamicMetaObjectProvider);
        private readonly LookupSlim<Type, DeferredValue<ValuePair, ChainSegment[]>> _membersCache = new LookupSlim<Type, DeferredValue<ValuePair, ChainSegment[]>>();
        private readonly ReflectionMemberAccessor _reflectionMemberAccessor;

        private static object[] _dependencies;
        
        public ObjectDescriptorProvider(ICompiledHandlebarsConfiguration configuration)
        {
            _dynamicObjectDescriptor = new DynamicObjectDescriptor(configuration);
            _reflectionMemberAccessor = new ReflectionMemberAccessor(configuration.AliasProviders);

            _dependencies = new object[]
            {
                _membersCache,
                configuration.TemplateContext
            };
        }
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (type == StringType)
            {
                value = ObjectDescriptor.Empty;
                return false;
            }

            if (_dynamicMetaObjectProviderType.IsAssignableFrom(type))
            {
                if (_dynamicObjectDescriptor.TryGetDescriptor(type, out var dynamicDescriptor))
                {
                    var mergedMemberAccessor = new MergedMemberAccessor(_reflectionMemberAccessor, dynamicDescriptor.MemberAccessor);
                    value = new ObjectDescriptor(type, 
                        mergedMemberAccessor,
                        self => new ObjectIterator(self), 
                        (descriptor, o) => GetProperties(descriptor, o).OfType<ChainSegment>().Concat(dynamicDescriptor.GetProperties(descriptor, o).Cast<ChainSegment>()), 
                        _dependencies
                    );

                    return true;
                }
                
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            value = new ObjectDescriptor(type, _reflectionMemberAccessor, self => new ObjectIterator(self), GetProperties, _dependencies);

            return true;
        }
        
        private static readonly Func<Type, TemplateContext, DeferredValue<ValuePair, ChainSegment[]>> DescriptorValueFactory =
            (key, state) =>
            {
                return new DeferredValue<ValuePair, ChainSegment[]>(new ValuePair(key, state), deps =>
                {
                    var properties = deps.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(o => o.CanRead && o.GetIndexParameters().Length == 0);
                    var fields = deps.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    
                    return properties.Cast<MemberInfo>()
                        .Concat(fields)
                        .Select(o => o.Name)
                        .Select(o => ChainSegment.Create(deps.TemplateContext, o))
                        .ToArray();
                });
            };

        private static readonly Func<ObjectDescriptor, object, IEnumerable> GetProperties = (descriptor, o) =>
        {
            var cache = (LookupSlim<Type, DeferredValue<ValuePair, ChainSegment[]>>) descriptor.Dependencies[0];
            return cache.GetOrAdd(descriptor.DescribedType, DescriptorValueFactory, (TemplateContext) descriptor.Dependencies[1]).Value;
        };
        
        private readonly struct ValuePair
        {
            public readonly TemplateContext TemplateContext;
            public readonly Type Type;

            public ValuePair(Type type, TemplateContext templateContext)
            {
                Type = type;
                TemplateContext = templateContext;
            }
        }
    }
}