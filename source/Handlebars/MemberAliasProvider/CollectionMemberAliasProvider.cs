using System;
using System.Collections;
using System.Linq;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;

namespace HandlebarsDotNet.MemberAliasProvider
{
    internal sealed class CollectionMemberAliasProvider : IMemberAliasProvider
    {
        private readonly ICompiledHandlebarsConfiguration _configuration;
        private static readonly ChainSegment Count = ChainSegment.Create(TemplateContext.Shared, "Count");
        private static readonly ChainSegment Length = ChainSegment.Create(TemplateContext.Shared, "Length");

        public CollectionMemberAliasProvider(ICompiledHandlebarsConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public bool TryGetMemberByAlias(object instance, Type targetType, ChainSegment memberAlias, out object value)
        {
            switch (instance)
            {
                case Array array when memberAlias.Equals(Count):
                    value = array.Length;
                    return true;

                case ICollection array when memberAlias.Equals(Length):
                    value = array.Count;
                    return true;

                case IEnumerable enumerable when _configuration.ObjectDescriptorProvider.TryGetDescriptor(targetType, out var descriptor) && descriptor.GetProperties != null:
                    var properties = descriptor.GetProperties(descriptor, enumerable);
                    var property = properties.OfType<ChainSegment>()
                        .FirstOrDefault(o => o.Equals("length") || o.Equals("count"));

                    if (property != null && descriptor.MemberAccessor.TryGetValue(enumerable, targetType, property.ToString(), out value)) return true;
                    
                    value = null;
                    return false;

                default:
                    value = null;
                    return false;
            }
        }
    }
}