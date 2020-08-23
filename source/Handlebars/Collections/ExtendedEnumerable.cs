using System;
using System.Collections;
using System.Collections.Generic;
using HandlebarsDotNet.Adapters;

namespace HandlebarsDotNet.Collections
{
    /// <summary>
    /// Wraps <see cref="IEnumerable"/> and provide additional information about the iteration via <see cref="EnumeratorValue{T}"/>
    /// </summary>
    internal sealed class ExtendedEnumerable2<T>
    {
        private Enumerator _enumerator;

        public ExtendedEnumerable2(IEnumerable enumerable)
        {
            _enumerator = new Enumerator(enumerable.GetEnumerator());
        }
            
        public ref Enumerator GetEnumerator()
        {
            return ref _enumerator;
        }

        internal struct Enumerator : IDisposable
        {
            private readonly IEnumerator _enumerator;
            private ReusableRef<T> _next;
            private int _index;

            public Enumerator(IEnumerator enumerator) : this()
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
                        ? new EnumeratorValue<T>(_next.Value, _index++, true)
                        : EnumeratorValue<T>.Empty;

                    _next = null;
                    return;
                }

                if (_next == null)
                {
                    _next = RefPool<T>.Shared.Create((T) _enumerator.Current);
                    return;
                }

                Current = new EnumeratorValue<T>(_next.Value, _index++, false);
                _next.Value = (T) _enumerator.Current;
            }

            public void Dispose()
            {
                RefPool<T>.Shared.Return(_next);
            }
        }
    }
    
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

        internal struct Enumerator : IDisposable
        {
            private readonly IEnumerator<T> _enumerator;
            private ReusableRef<T> _next;
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
                        ? new EnumeratorValue<T>(_next.Value, _index++, true)
                        : EnumeratorValue<T>.Empty;

                    _next = null;
                    return;
                }

                if (_next == null)
                {
                    _next = RefPool<T>.Shared.Create(_enumerator.Current);
                    return;
                }

                Current = new EnumeratorValue<T>(_next.Value, _index++, false);
                _next.Value = _enumerator.Current;
            }

            public void Dispose()
            {
                RefPool<T>.Shared.Return(_next);
            }
        }
    }

    internal readonly struct EnumeratorValue<T>
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