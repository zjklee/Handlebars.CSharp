using System.Collections;
using System.Collections.Generic;

namespace HandlebarsDotNet.Collections
{
    /// <summary>
    /// Wraps <see cref="IEnumerable"/> and provide additional information about the iteration via <see cref="EnumeratorValue{T}"/>
    /// </summary>
    internal sealed class ExtendedEnumerable<T>
    {
        private Enumerator _enumerator;

        public ExtendedEnumerable(IEnumerable<T> enumerable)
        {
            _enumerator = new Enumerator(enumerable.GetEnumerator());
        }
            
        public ref Enumerator GetEnumerator()
        {
            return ref _enumerator;
        }

        public struct Enumerator
        {
            private readonly IEnumerator<T> _enumerator;
            private T _next;
            private int _index;

            public Enumerator(IEnumerator<T> enumerator) : this()
            {
                _enumerator = enumerator;
                PerformIteration();
            }
            
            public EnumeratorValue<T> Current { get; private set; }

            public bool MoveNext()
            {
                if (_next == null) return false;
                    
                PerformIteration();

                return true;
            }

            private void PerformIteration()
            {
                if (!_enumerator.MoveNext())
                {
                    Current = _next != null
                        ? new EnumeratorValue<T>(_next, _index++, true)
                        : EnumeratorValue<T>.Empty;

                    _next = default;
                    return;
                }

                if (_next == null)
                {
                    _next = _enumerator.Current;
                    return;
                }

                Current = new EnumeratorValue<T>(_next, _index++, false);
                _next = _enumerator.Current;
            }
        }
    }
    
    /// <summary>
    /// Wraps <see cref="IEnumerable"/> and provide additional information about the iteration via <see cref="EnumeratorValue{T}"/>
    /// </summary>
    internal ref struct ExtendedEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private T _next;
        private int _index;
        
        public ExtendedEnumerator(IEnumerator<T> enumerator) : this()
        {
            _enumerator = enumerator;
            PerformIteration();
        }

        public EnumeratorValue<T> Current;

        public bool MoveNext()
        {
            if (_next == null) return false;
                    
            PerformIteration();

            return true;
        }

        private void PerformIteration()
        {
            if (!_enumerator.MoveNext())
            {
                Current = _next != null
                    ? new EnumeratorValue<T>(_next, _index++, true)
                    : EnumeratorValue<T>.Empty;

                _next = default;
                return;
            }

            if (_next == null)
            {
                _next = _enumerator.Current;
                return;
            }

            Current = new EnumeratorValue<T>(_next, _index++, false);
            _next = _enumerator.Current;
        }
    }
    
    internal ref struct ExtendedEnumerator
    {
        private readonly IEnumerator _enumerator;
        private object _next;
        private int _index;
        
        public ExtendedEnumerator(IEnumerator enumerator) : this()
        {
            _enumerator = enumerator;
            PerformIteration();
        }

        public EnumeratorValue<object> Current;

        public bool MoveNext()
        {
            if (_next == null) return false;
                    
            PerformIteration();

            return true;
        }

        private void PerformIteration()
        {
            if (!_enumerator.MoveNext())
            {
                Current = _next != null
                    ? new EnumeratorValue<object>(_next, _index++, true)
                    : EnumeratorValue<object>.Empty;

                _next = default;
                return;
            }

            if (_next == null)
            {
                _next = _enumerator.Current;
                return;
            }

            Current = new EnumeratorValue<object>(_next, _index++, false);
            _next = _enumerator.Current;
        }
    }

    public readonly struct EnumeratorValue<T>
    {
        public static readonly EnumeratorValue<T> Empty = new EnumeratorValue<T>();
        
        public EnumeratorValue(T value, int index, bool isLast)
        {
            Value = value;
            Index = index;
            IsLast = isLast;
            IsFirst = index == 0;
        }

        public T Value { get; }
        public int Index { get; }
        public bool IsFirst { get; }
        public bool IsLast { get; }
    }
}