using System;
using System.Collections.Generic;

namespace HandlebarsDotNet
{
    internal class DictionaryPool<TKey, TValue> : InternalObjectPool<Dictionary<TKey, TValue>>
    {
        private static readonly Lazy<DictionaryPool<TKey, TValue>> Self = new Lazy<DictionaryPool<TKey, TValue>>(() => new DictionaryPool<TKey, TValue>());

        public static DictionaryPool<TKey, TValue> Shared => Self.Value;
        
        private DictionaryPool() : base(new Policy())
        {
        }

        public DictionaryPool(IInternalObjectPoolPolicy<Dictionary<TKey, TValue>> policy) : base(policy)
        {
            
        }
        
        private class Policy : IInternalObjectPoolPolicy<Dictionary<TKey, TValue>>
        {
            public Dictionary<TKey, TValue> Create()
            {
                return new Dictionary<TKey, TValue>(EqualityComparer<TKey>.Default);
            }

            public bool Return(Dictionary<TKey, TValue> item)
            {
                item.Clear();
                return true;
            }
        }
    }
}