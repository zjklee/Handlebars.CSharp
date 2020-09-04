using System;
using System.Collections;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.ObjectDescriptors
{
    public class ObjectDescriptor<T> : ObjectDescriptor
    {
        public ObjectDescriptor(
            IMemberAccessor memberAccessor, 
            Func<ObjectDescriptor, HandlebarsDotNet.Iterators.Iterator> iterator, 
            Func<ObjectDescriptor<T>, T, IEnumerable> getProperties) 
            : base(typeof(T), 
                memberAccessor, 
                iterator, 
                (descriptor, o) => getProperties((ObjectDescriptor<T>) descriptor, (T) o))
        {
        }
    }

    /// <summary>
    /// Provides meta-information about <see cref="Type"/>
    /// </summary>
    public class ObjectDescriptor : IEquatable<ObjectDescriptor>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly ObjectDescriptor Empty = new ObjectDescriptor();

        private readonly bool _isNotEmpty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="describedType">Returns type described by this instance of <see cref="ObjectDescriptor"/></param>
        /// <param name="memberAccessor"><see cref="IMemberAccessor"/> associated with the <see cref="ObjectDescriptor"/></param>
        /// <param name="getProperties">Factory enabling receiving properties of specific instance</param>
        /// <param name="shouldEnumerate">Specifies whether the type should be treated as <see cref="System.Collections.IEnumerable"/></param>
        /// <param name="dependencies"></param>
        public ObjectDescriptor(
            Type describedType, 
            IMemberAccessor memberAccessor,
            Func<ObjectDescriptor, object, IEnumerable> getProperties,
            params object[] dependencies
        )
        {
            DescribedType = describedType;
            GetProperties = getProperties;
            MemberAccessor = memberAccessor;
            Dependencies = dependencies;

            _isNotEmpty = true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="describedType">Returns type described by this instance of <see cref="ObjectDescriptor"/></param>
        /// <param name="memberAccessor"><see cref="IMemberAccessor"/> associated with the <see cref="ObjectDescriptor"/></param>
        /// <param name="getProperties"></param>
        /// <param name="iterator">
        /// Factory enabling receiving properties of specific instance.
        /// <para>Known behavior is to return <c>object</c> for Array-like enumeration or return <c>KeyValuePair{string,object}</c> for object-like enumeration</para>
        /// </param>
        /// <param name="shouldEnumerate"></param>
        /// <param name="dependencies"></param>
        public ObjectDescriptor(
            Type describedType, 
            IMemberAccessor memberAccessor,
            Func<ObjectDescriptor, HandlebarsDotNet.Iterators.Iterator> iterator,
            Func<ObjectDescriptor, object, IEnumerable> getProperties,
            params object[] dependencies
        )
        {
            DescribedType = describedType;
            MemberAccessor = memberAccessor;
            GetProperties = getProperties;
            Dependencies = dependencies;
            Iterator = iterator(this);

            _isNotEmpty = true;
        }
        
        public ObjectDescriptor(
            Type describedType, 
            IMemberAccessor memberAccessor,
            Func<ObjectDescriptor, HandlebarsDotNet.Iterators.Iterator> iterator,
            Func<ObjectDescriptor, object, IEnumerable> getProperties
        )
        {
            DescribedType = describedType;
            MemberAccessor = memberAccessor;
            Iterator = iterator(this);
            GetProperties = getProperties;

            _isNotEmpty = true;
        }

        private ObjectDescriptor(){ }
        
        /// <summary>
        /// Contains dependencies for <see cref="GetProperties"/> delegate
        /// </summary>
        public readonly object[] Dependencies;

        /// <summary>
        /// Returns type described by this instance of <see cref="ObjectDescriptor"/>
        /// </summary>
        public readonly Type DescribedType;

        /// <summary>
        /// Factory enabling receiving properties of specific instance   
        /// </summary>
        public readonly Func<ObjectDescriptor, object, IEnumerable> GetProperties;

        /// <summary>
        /// <see cref="IMemberAccessor"/> associated with the <see cref="ObjectDescriptor"/>
        /// </summary>
        public readonly IMemberAccessor MemberAccessor;

        public readonly HandlebarsDotNet.Iterators.Iterator Iterator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object AccessMember(object instance, ChainSegment segment)
        {
            return MemberAccessor.TryGetValue(instance, DescribedType, segment, out var value) ? value : null;
        }
        
        /// <inheritdoc />
        public bool Equals(ObjectDescriptor other)
        {
            return _isNotEmpty == other?._isNotEmpty && DescribedType == other.DescribedType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ObjectDescriptor other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (_isNotEmpty.GetHashCode() * 397) ^ (DescribedType?.GetHashCode() ?? 0);
            }
        }
        
        /// <inheritdoc cref="Equals(HandlebarsDotNet.ObjectDescriptors.ObjectDescriptor)"/>
        public static bool operator ==(ObjectDescriptor a, ObjectDescriptor b)
        {
            return Equals(a, b);
        }
        
        /// <inheritdoc cref="Equals(HandlebarsDotNet.ObjectDescriptors.ObjectDescriptor)"/>
        public static bool operator !=(ObjectDescriptor a, ObjectDescriptor b)
        {
            return !Equals(a, b);
        }

        /// <inheritdoc />
        public override string ToString() => DescribedType.ToString();
    }
}