using System;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.ObjectDescriptors;
using Newtonsoft.Json.Linq;

namespace HandlebarsDotNet.Extension.NewtonsoftJson
{
    internal class JArrayDescriptorProvider : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(JArray);
        
        private readonly ObjectDescriptorProvider _objectDescriptorProvider;
        private static readonly JArrayMemberAccessor JArrayMemberAccessor = new JArrayMemberAccessor();
        
        public JArrayDescriptorProvider(ObjectDescriptorProvider objectDescriptorProvider)
        {
            _objectDescriptorProvider = objectDescriptorProvider;
        }
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (Type != type)
            {
                value = ObjectDescriptor.Empty;
                return false;
            }

            if (!_objectDescriptorProvider.TryGetDescriptor(type, out var objectDescriptor))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            var memberAccessor = new MergedMemberAccessor(JArrayMemberAccessor, objectDescriptor.MemberAccessor);
            value = new ObjectDescriptor(typeof(JArray), memberAccessor, null, true);
            
            return true;
        }
    }
}