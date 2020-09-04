using System;
using System.Runtime.CompilerServices;

namespace HandlebarsDotNet.Adapters
{
    /// <summary>
    /// Wrapper for other values. Used as a reference storage for other references or values.
    /// <para>Should be used for low-level optimizations.</para>
    /// </summary>
    public abstract class Ref
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ref<T> Create<T>(T item) => new Ref<T>(item);
                
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
        
        public abstract Type InnerType { get; }
    }

    /// <inheritdoc cref="Ref"/>
    /// <typeparam name="T"></typeparam>
    public class Ref<T> : Ref
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public Ref(T value) => Value = value;

        /// <summary>
        /// 
        /// </summary>
        public T Value { get; set; }

        public override object GetValue() => Value;

        public override void SetValue(object value) => Value = (T) value;

        public override Type InnerType => Value?.GetType() ?? typeof(T);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator T(Ref<T> value) => value.Value;
        
        /// <summary>
        /// Returns string representation of <see cref="Ref"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value.ToString();
    }
}