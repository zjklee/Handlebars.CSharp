using System;
using System.Collections;
using System.Reflection;
using HandlebarsDotNet.Iterators;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet.ObjectDescriptors
{
    internal class DictionaryObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(IDictionary);
        private static readonly DictionaryMemberAccessor DictionaryMemberAccessor = new DictionaryMemberAccessor();
        private static readonly Func<ObjectDescriptor, object, IEnumerable> GetProperties = (descriptor, arg) => ((IDictionary) arg).Keys;
        private static readonly MethodInfo Factory = typeof(DictionaryObjectDescriptor)
            .GetMethod(nameof(ObjectDescriptorFactory), BindingFlags.Static | BindingFlags.NonPublic);
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!Type.IsAssignableFrom(type))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            value = (ObjectDescriptor) Factory.MakeGenericMethod(type).Invoke(null, ArrayEx.Empty<object>());
            return true;
        }

        private static ObjectDescriptor ObjectDescriptorFactory<T>() 
            where T : class, IDictionary
        {
            return new ObjectDescriptor<T>(DictionaryMemberAccessor, self => new DictionaryIterator<T>(self), GetProperties);
        }
    }
}