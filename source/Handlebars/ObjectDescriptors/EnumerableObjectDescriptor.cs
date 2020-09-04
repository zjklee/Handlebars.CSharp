using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HandlebarsDotNet.Iterators;
using HandlebarsDotNet.MemberAccessors;
using HandlebarsDotNet.MemberAccessors.EnumerableAccessors;

namespace HandlebarsDotNet.ObjectDescriptors
{
    internal class EnumerableObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly Type Type = typeof(IEnumerable);
        private static readonly Type StringType = typeof(string);

        private static readonly MethodInfo ArrayObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(ArrayObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);
        
        private static readonly MethodInfo ListObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(ListObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);
        
        private static readonly MethodInfo NonGenericListObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(NonGenericListObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);
        
        private static readonly MethodInfo CollectionObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(CollectionObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);
        
        private static readonly MethodInfo NonGenericCollectionObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(NonGenericCollectionObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);
        
        private static readonly MethodInfo EnumerableObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(EnumerableObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);
        
        private static readonly MethodInfo NonGenericEnumerableObjectDescriptorFactoryMethodInfo = typeof(EnumerableObjectDescriptor)
            .GetMethod(nameof(NonGenericEnumerableObjectDescriptorFactory), BindingFlags.NonPublic | BindingFlags.Static);

        private readonly IObjectDescriptorProvider _descriptorProvider;
        
        public EnumerableObjectDescriptor(IObjectDescriptorProvider descriptorProvider)
        {
            _descriptorProvider = descriptorProvider;
        }
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!(type != StringType && Type.IsAssignableFrom(type)))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            if (!_descriptorProvider.TryGetDescriptor(type, out value))
            {
                value = ObjectDescriptor.Empty;
                return false;
            }
            
            var enumerableMemberAccessor = EnumerableMemberAccessor.Create(type);
            var mergedMemberAccessor = new MergedMemberAccessor(enumerableMemberAccessor, value.MemberAccessor);

            var parameters = new object[]{ mergedMemberAccessor, value };
            return TryCreateArrayDescriptor(type, parameters, out value)
                   || TryCreateDescriptorFromOpenGeneric(type, typeof(IList<>), parameters, ListObjectDescriptorFactoryMethodInfo, out value)
                   || TryCreateDescriptorFromOpenGeneric(type, typeof(ICollection<>), parameters, CollectionObjectDescriptorFactoryMethodInfo, out value)
                   || TryCreateDescriptorFromOpenGeneric(type, typeof(ICollection<>), parameters, EnumerableObjectDescriptorFactoryMethodInfo, out value)
                   || TryCreateDescriptor(type, typeof(IList), parameters, NonGenericListObjectDescriptorFactoryMethodInfo, out value)
                   || TryCreateDescriptor(type, typeof(ICollection), parameters, NonGenericCollectionObjectDescriptorFactoryMethodInfo, out value)
                   || TryCreateDescriptor(type, typeof(IEnumerable), parameters, NonGenericEnumerableObjectDescriptorFactoryMethodInfo, out value);
        }

        private static bool TryCreateArrayDescriptor(Type type, object[] parameters, out ObjectDescriptor value)
        {
            if (type.IsArray)
            {
                value = (ObjectDescriptor) ArrayObjectDescriptorFactoryMethodInfo
                    .MakeGenericMethod(type.GetElementType())
                    .Invoke(null, parameters);

                return true;
            }

            value = null;
            return false;
        }

        private static bool TryCreateDescriptorFromOpenGeneric(Type type, Type openGenericType, object[] parameters, MethodInfo method, out ObjectDescriptor descriptor)
        {
            if (type.IsAssignableToGenericType(openGenericType, out var genericType))
            {
                descriptor = (ObjectDescriptor) method
                    .MakeGenericMethod(type, genericType.GenericTypeArguments[0])
                    .Invoke(null, parameters);

                return true;
            }

            descriptor = null;
            return false;
        }
        
        private static bool TryCreateDescriptor(Type type, Type targetType, object[] parameters, MethodInfo method, out ObjectDescriptor descriptor)
        {
            if (targetType.IsAssignableFrom(type))
            {
                descriptor = (ObjectDescriptor) method
                    .MakeGenericMethod(type)
                    .Invoke(null, parameters);

                return true;
            }

            descriptor = null;
            return false;
        }

        private static ObjectDescriptor ArrayObjectDescriptorFactory<TValue>(IMemberAccessor accessor, ObjectDescriptor descriptor)
        {
            return new ObjectDescriptor<TValue[]>(accessor, self => new ArrayIterator<TValue>(self), descriptor.GetProperties);
        }
        
        private static ObjectDescriptor ListObjectDescriptorFactory<T, TValue>(IMemberAccessor accessor, ObjectDescriptor descriptor) 
            where T : class, IList<TValue>
        {
            return new ObjectDescriptor<T>(accessor, self => new ListIterator<T, TValue>(self), descriptor.GetProperties);
        }
        
        private static ObjectDescriptor NonGenericListObjectDescriptorFactory<T>(IMemberAccessor accessor, ObjectDescriptor descriptor) 
            where T : class, IList
        {
            return new ObjectDescriptor<T>(accessor, self => new ListIterator<T>(self), descriptor.GetProperties);
        }
        
        private static ObjectDescriptor CollectionObjectDescriptorFactory<T, TValue>(IMemberAccessor accessor, ObjectDescriptor descriptor) 
            where T : class, ICollection<TValue>
        {
            return new ObjectDescriptor<T>(accessor, self => new CollectionIterator<T, TValue>(self), descriptor.GetProperties);
        }
        
        private static ObjectDescriptor NonGenericCollectionObjectDescriptorFactory<T>(IMemberAccessor accessor, ObjectDescriptor descriptor) 
            where T : class, ICollection
        {
            return new ObjectDescriptor<T>(accessor, self => new CollectionIterator<T>(self), descriptor.GetProperties);
        }
        
        private static ObjectDescriptor EnumerableObjectDescriptorFactory<T, TValue>(IMemberAccessor accessor, ObjectDescriptor descriptor) 
            where T : class, IEnumerable<TValue>
        {
            return new ObjectDescriptor<T>(accessor, self => new EnumerableIterator<T, TValue>(self), descriptor.GetProperties);
        }
        
        private static ObjectDescriptor NonGenericEnumerableObjectDescriptorFactory<T>(IMemberAccessor accessor, ObjectDescriptor descriptor) 
            where T : class, IEnumerable
        {
            return new ObjectDescriptor<T>(accessor, self => new EnumerableIterator<T>(self), descriptor.GetProperties);
        }
    }
}