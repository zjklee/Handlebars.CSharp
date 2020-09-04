using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.ObjectDescriptors
{
    internal class DynamicObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(IDynamicMetaObjectProvider);
        private static readonly DynamicMemberAccessor DynamicMemberAccessor = new DynamicMemberAccessor();
        private static readonly Func<ObjectDescriptor, object, IEnumerable> GetProperties = (descriptor, o) =>
        {
            var templateContext = (TemplateContext) descriptor.Dependencies[0];
            var dynamicMemberNames = ((IDynamicMetaObjectProvider) o)
                .GetMetaObject(Expression.Constant(o))
                .GetDynamicMemberNames();
            
            return Enumerate(templateContext, dynamicMemberNames);

            static IEnumerable<ChainSegment> Enumerate(TemplateContext templateContext, IEnumerable<string> enumerable)
            {
                foreach (var name in enumerable)
                {
                    yield return templateContext.CreateChainSegment(name);
                }
            }
        };

        private readonly object[] _dependencies;
        
        public DynamicObjectDescriptor(ICompiledHandlebarsConfiguration configuration)
        {
            _dependencies = new object[]{ configuration.TemplateContext };
        }

        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!Type.IsAssignableFrom(type))
            {
                value = ObjectDescriptor.Empty;;
                return false;
            }
            
            value = new ObjectDescriptor(type, DynamicMemberAccessor, GetProperties, _dependencies);

            return true;
        }
    }
}