using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace HandlebarsDotNet
{
    internal static class ObjectPoolExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DisposableContainer<T> Use<T>(this InternalObjectPool<T> objectPool) where T : class
        {
            return new DisposableContainer<T>(objectPool.Get(), objectPool.Return);
        }
    }

    internal class InternalObjectPool<T>
    {
        private readonly IInternalObjectPoolPolicy<T> _policy;
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public InternalObjectPool(IInternalObjectPoolPolicy<T> policy)
        {
            Handlebars.Disposables.Add(new Disposer(this));
            
            _policy = policy;

            for (var i = 0; i < 5; i++) Return(_policy.Create());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
        {
            if (_queue.TryDequeue(out var item) && !ReferenceEquals(item, null))
            {
                return item;
            }

            return _policy.Create();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T obj)
        {
            if(obj == null) return;
            if (!_policy.Return(obj)) return;
            
            _queue.Enqueue(obj);
        }

        private sealed class Disposer : IDisposable
        {
            private readonly InternalObjectPool<T> _target;

            public Disposer(InternalObjectPool<T> target) => _target = target;

            public void Dispose() => _target._queue = new ConcurrentQueue<T>();
        }
    }

    internal interface IInternalObjectPoolPolicy<T>
    {
        T Create();
        bool Return(T item);
    }
}