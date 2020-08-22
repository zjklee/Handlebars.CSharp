using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HandlebarsDotNet.Adapters
{
    /// <summary>
    /// 
    /// </summary>
    public static class RefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Ref<T> AsRef<T>(this T value)
        {
            return new Ref<T>(value);
        }
    } 
    
    /// <summary>
    /// Wrapper for other values. Used as a reference storage for other references or values.
    /// </summary>
    public abstract class Ref
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract object GetValue();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public abstract void SetValue(object value);
    }

    /// <inheritdoc cref="Ref"/>
    /// <typeparam name="T"></typeparam>
    public sealed class Ref<T> : Ref, IEquatable<Ref<T>>, IEquatable<T>
    {
        private T _value;
        private Ref<T> _ref;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public Ref(T value)
        {
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Value
        {
            get
            {
                if (_ref != null) return _ref.Value;
                return _value;
            }
            set
            {
                _value = value;
                _ref = null;
            }
        }

        public override object GetValue()
        {
            if (_ref != null) return _ref.GetValue();
            return _value;
        }

        public override void SetValue(object value)
        {
            if (value is Ref<T> @ref)
            {
                _ref = @ref;
                _value = default;
                return;
            }
            
            _value = (T) value;
            _ref = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ref"></param>
        /// <returns></returns>
        public static implicit operator T(Ref<T> @ref) => @ref.Value;
        
        /// <summary>
        /// Returns string representation of <see cref="Value"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value.ToString();
        
        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(T other) => EqualityComparer<T>.Default.Equals(Value, other);

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(Ref<T> other)
        {
            if (ReferenceEquals(other, null)) return false;
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Ref<T> @ref && Equals(@ref);
        }

        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);
    }
}