using System;
using System.Collections;
using System.Collections.Generic;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.ObjectDescriptors
{
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
            Func<ObjectDescriptor, object, IEnumerable<object>> getProperties,
            bool shouldEnumerate = false,
            params object[] dependencies
        )
        {
            DescribedType = describedType;
            GetProperties = getProperties;
            MemberAccessor = memberAccessor;
            ShouldEnumerate = shouldEnumerate;
            Dependencies = dependencies;

            _isNotEmpty = true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="describedType">Returns type described by this instance of <see cref="ObjectDescriptor"/></param>
        /// <param name="memberAccessor"><see cref="IMemberAccessor"/> associated with the <see cref="ObjectDescriptor"/></param>
        /// <param name="getEnumerator">
        /// Factory enabling receiving properties of specific instance.
        /// <para>Known behavior is to return <c>object</c> for Array-like enumeration or return <c>KeyValuePair{string,object}</c> for object-like enumeration</para>
        /// </param>
        /// <param name="dependencies"></param>
        public ObjectDescriptor(
            Type describedType, 
            IMemberAccessor memberAccessor,
            Func<ObjectDescriptor, object, IEnumerable> getEnumerator,
            params object[] dependencies
        )
        {
            DescribedType = describedType;
            GetEnumerator = getEnumerator;
            MemberAccessor = memberAccessor;
            ShouldEnumerate = false;
            Dependencies = dependencies;

            _isNotEmpty = true;
        }

        private ObjectDescriptor(){ }

        /// <summary>
        /// Specifies whether the type should be treated as <see cref="System.Collections.IEnumerable"/>
        /// </summary>
        public readonly bool ShouldEnumerate;
        
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
        public readonly Func<ObjectDescriptor, object, IEnumerable<object>> GetProperties;
        
        /// <summary>
        /// Factory enabling receiving properties of specific instance   
        /// </summary>
        public readonly Func<ObjectDescriptor, object, IEnumerable> GetEnumerator;
        
        /// <summary>
        /// <see cref="IMemberAccessor"/> associated with the <see cref="ObjectDescriptor"/>
        /// </summary>
        public readonly IMemberAccessor MemberAccessor;

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