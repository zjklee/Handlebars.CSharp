using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HandlebarsDotNet.Collections
{
    /// <summary>
    /// Append-only data structure of a fixed size that provides dictionary-like lookup capabilities.
    /// <para>Collection does not guaranty key uniqueness when <see cref="Add"/> method is used.</para>
    /// <para>Performance of <see cref="AddOrReplace"/>, <see cref="ContainsKey(in TKey)"/> and <see cref="TryGetValue(in TKey, out TValue)"/>
    /// starts to degrade as number of items comes closer to <see cref="Capacity"/>.</para>
    /// <para><see cref="TryGetValue(in EntryIndex(TKey), out TValue)"/> and <see cref="ContainsKey(in EntryIndex(TKey)"/> always performs at constant time.</para>
    /// </summary>
    public class FixedSizeDictionary<TKey, TValue, TComparer>
        where TKey : notnull
        where TValue : notnull
        where TComparer : notnull, IEqualityComparer<TKey>
    {
        private const int MaximumSize = 1024;

        private readonly int _bucketMask;

        private readonly Entry[] _entries;
        private readonly Bucket[] _buckets;

        private readonly TComparer _comparer;

        private int _version;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Actual capacity would be the size ^2 and multiplied by itself. Maximum size is <c>1024</c> with <c>1048576</c> capacity.</param>
        /// <param name="comparer"></param>
        public FixedSizeDictionary(int size, TComparer comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (size > MaximumSize) throw new ArgumentException($" cannot be greater then {MaximumSize}", nameof(size));

            // size is always ^2.
            //size = HashHelpers.GetPrime(size);//AlignSize(size);
            size = AlignSize(size);
            _comparer = comparer;
            _bucketMask = size - 1;
            _version = 0;

            _buckets = new Bucket[size];
            InitializeBuckets(_buckets, _bucketMask);

            _entries = new Entry[size * size];

            static int AlignSize(int size)
            {
                size--;
                size |= size >> 1;
                size |= size >> 2;
                size |= size >> 4;
                size |= size >> 8;
                size |= size >> 16;
                size++;

                return size;
            }

            static void InitializeBuckets(Bucket[] buckets, int bucketMask)
            {
                for (var i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = new Bucket(-1, i * bucketMask);
                }
            }
        }

        /// <summary>
        /// Amount of items can be added to the dictionary
        /// </summary>
        public int Capacity => _entries.Length;

        /// <summary>
        /// Calculates current index for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetIndex(TKey key, out EntryIndex<TKey> index)
        {
            var hash = _comparer.GetHashCode(key);
            var bucketIndex = hash & _bucketMask;

            var entryIndex = bucketIndex * _bucketMask;

            if (_buckets[bucketIndex].Version != _version)
            {
                index = new EntryIndex<TKey>(bucketIndex, key, _version);
                return true;
            }

            var entry = _entries[entryIndex];
            if (entry.Version != _version || hash == entry.Hash && _comparer.Equals(key, entry.Key))
            {
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return true;
            }

            while (entry.Next != -1)
            {
                entry = _entries[entry.Next];
                if (!entry.IsNotDefault) break;
                if (entry.Version == _version && (hash != entry.Hash || !_comparer.Equals(key, entry.Key))) continue;

                index = new EntryIndex<TKey>(entry.Index, key, _version);
                return true;
            }

            index = new EntryIndex<TKey>();
            return false;
        }

        /// <summary>
        /// Checks key existence at guarantied O(1) ignoring actual key comparison
        /// </summary>
        public bool ContainsKey(in EntryIndex<TKey> keyIndex)
        {
            if (keyIndex.Version != _version) return false;

            var entry = _entries[keyIndex.Index];

            return entry.IsNotDefault && entry.Version == _version;
        }

        /// <summary>
        /// Checks key existence at best O(1) and worst O(m) where 'm' is number of collisions 
        /// </summary>
        public bool ContainsKey(in TKey key)
        {
            var hash = _comparer.GetHashCode(key);
            var bucketIndex = hash & _bucketMask;

            if (_buckets[bucketIndex].Version != _version) return false;

            var entryIndex = bucketIndex * _bucketMask;

            var entry = _entries[entryIndex];
            if (!entry.IsNotDefault || entry.Version != _version) return false;
            if (hash == entry.Hash && _comparer.Equals(key, entry.Key))
            {
                return true;
            }

            while (entry.Next != -1)
            {
                entry = _entries[entry.Next];
                if (!entry.IsNotDefault || entry.Version != _version) return false;
                if (hash == entry.Hash && _comparer.Equals(key, entry.Key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs lookup at guarantied O(1) ignoring actual key comparison
        /// </summary>
        public bool TryGetValue(in EntryIndex<TKey> keyIndex, out TValue value)
        {
            if (keyIndex.Version != _version)
            {
                value = default;
                return false;
            }

            var entry = _entries[keyIndex.Index];
            if (entry.IsNotDefault && entry.Version == _version)
            {
                value = entry.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Performs lookup at best O(1) and worst O(m) where 'm' is number of collisions
        /// </summary>
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var hash = _comparer.GetHashCode(key);
            var bucketIndex = hash & _bucketMask;

            if (_buckets[bucketIndex].Version != _version)
            {
                value = default;
                return false;
            }

            var entryIndex = bucketIndex * _bucketMask;

            var entry = _entries[entryIndex];
            if (!entry.IsNotDefault || entry.Version != _version)
            {
                value = default;
                return false;
            }

            if (hash == entry.Hash && _comparer.Equals(key, entry.Key))
            {
                value = entry.Value;
                return true;
            }

            while (entry.Next != -1)
            {
                entry = _entries[entry.Next];
                if (!entry.IsNotDefault || entry.Version != _version)
                {
                    value = default;
                    return false;
                }

                if (hash == entry.Hash && _comparer.Equals(key, entry.Key))
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Adds or replaces an item at best O(1) and worst O(m) where 'm' is number of collisions
        /// </summary>
        /// <exception cref="InvalidOperationException">Item cannot be added due to capacity constraint.</exception>
        public void AddOrReplace(in TKey key, in TValue value, out EntryIndex<TKey> index)
        {
            var hash = _comparer.GetHashCode(key);
            var bucketIndex = hash & _bucketMask;

            var entryIndex = bucketIndex * _bucketMask;

            if (_buckets[bucketIndex].Version != _version)
            {
                _buckets[bucketIndex].Version = _version;
                _buckets[bucketIndex].Head = entryIndex;
                _buckets[bucketIndex].Count = 1;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            var entry = _entries[entryIndex];
            if (!entry.IsNotDefault || entry.Version != _version)
            {
                _buckets[bucketIndex].Head = entryIndex;
                _buckets[bucketIndex].Count = 1;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            if (hash == entry.Hash && _comparer.Equals(key, entry.Key))
            {
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                _entries[entryIndex].Value = value;
                return;
            }

            while (entry.Next != -1)
            {
                entry = _entries[entry.Next];
                if (entry.Version != _version)
                {
                    _buckets[bucketIndex].Head = entry.Index;
                    _buckets[bucketIndex].Count++;
                    _entries[entry.Index] = new Entry(hash, entry.Index, key, value, _version);
                    index = new EntryIndex<TKey>(entry.Index, key, _version);
                    return;
                }

                if (hash == entry.Hash && _comparer.Equals(key, entry.Key))
                {
                    index = new EntryIndex<TKey>(entry.Index, key, _version);
                    _entries[entry.Index].Value = value;
                    return;
                }
            }

            ref var entryReference = ref _entries[entry.Index];
            entryIndex = entryReference.Index + 1;

            for (; entryIndex < _entries.Length; entryIndex++)
            {
                entry = _entries[entryIndex];
                if (entry.IsNotDefault && entry.Version == _version) continue;

                entryReference.Next = entryIndex;
                _buckets[bucketIndex].Head = entryIndex;
                _buckets[bucketIndex].Count++;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            entryIndex = (bucketIndex * _bucketMask) - 1;
            for (; entryIndex >= 0; entryIndex--)
            {
                entry = _entries[entryIndex];
                if (entry.IsNotDefault && entry.Version == _version) continue;

                entryReference.Next = entryIndex;
                _buckets[bucketIndex].Head = entryIndex;
                _buckets[bucketIndex].Count++;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            throw new InvalidOperationException("Item cannot be added due to capacity constraint.");
        }

        /// <summary>
        /// Adds an item at best O(1) and worst O(m) where 'm' is number of items outside of bucket
        /// </summary>
        /// <exception cref="InvalidOperationException">Item cannot be added due to capacity constraint.</exception>
        public void Add(in TKey key, in TValue value, out EntryIndex<TKey> index)
        {
            var hash = _comparer.GetHashCode(key);
            var bucketIndex = hash & _bucketMask;
            ref var bucket = ref _buckets[bucketIndex];

            var entryIndex = bucketIndex * _bucketMask;

            if (bucket.Version != _version)
            {
                bucket.Version = _version;
                bucket.Head = entryIndex;
                _buckets[bucketIndex].Count = 1;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            ref var entryReference = ref _entries[bucket.Head];
            entryIndex = entryReference.Index + 1;

            Entry entry;
            for (; entryIndex < _entries.Length; entryIndex++)
            {
                entry = _entries[entryIndex];
                if (entry.IsNotDefault && entry.Version == _version) continue;

                entryReference.Next = entryIndex;
                bucket.Head = entryIndex;
                _buckets[bucketIndex].Count++;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            entryIndex = bucketIndex * _bucketMask - 1;
            for (; entryIndex >= 0; entryIndex--)
            {
                entry = _entries[entryIndex];
                if (entry.IsNotDefault && entry.Version == _version) continue;

                entryReference.Next = entryIndex;
                bucket.Head = entryIndex;
                _buckets[bucketIndex].Count++;
                _entries[entryIndex] = new Entry(hash, entryIndex, key, value, _version);
                index = new EntryIndex<TKey>(entryIndex, key, _version);
                return;
            }

            throw new InvalidOperationException("Item cannot be added due to capacity constraint.");
        }

        /// <summary>
        /// Gets or replaces item at a given index at O(1)
        /// </summary>
        /// <param name="entryIndex"></param>
        public TValue this[in EntryIndex<TKey> entryIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (entryIndex.Version != _version) return default;

                var entry = _entries[entryIndex.Index];
                var found = entry.IsNotDefault && entry.Version == _version;

                if (found) return entry.Value;
                return default;

            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (entryIndex.Version != _version) return;
                _entries[entryIndex.Index].Value = value;
            }
        }

        /// <summary>
        /// Copies items from one dictionary to another at O(n)
        /// </summary>
        /// <param name="destination"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(FixedSizeDictionary<TKey, TValue, TComparer> destination)
        {
            if (Capacity != destination.Capacity)
                throw new ArgumentException(" capacity should be equal to source dictionary", nameof(destination));

            destination._version = _version;

            for (var index = 0; index < _buckets.Length; index++)
            {
                destination._buckets[index] = _buckets[index];
            }

            for (int bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++)
            {
                if (_buckets[bucketIndex].Version != _version) continue;

                var entryIndex = bucketIndex * _bucketMask;

                var entry = _entries[entryIndex];
                destination[entry.Index] = entry;
                while (entry.Next != -1)
                {
                    entry = _entries[entry.Next];
                    destination[entry.Index] = entry;
                }
            }
        }

        public int Count => _buckets.Where(o => o.Version == _version).Sum(o => o.Count);

        /// <summary>
        /// Performs fast cleanup without cleaning internal storage (does not make objects available for GC)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => ++_version;

        /// <summary>
        /// Performs full cleanup
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (var bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++)
            {
                var entryIndex = bucketIndex * _bucketMask;

                var entry = _entries[entryIndex];
                _entries[entryIndex] = new Entry();
                while (entry.Next != -1)
                {
                    entry = _entries[entry.Next];
                    _entries[entry.Index] = new Entry();
                }
            }

            ++_version;
        }

        private Entry this[in int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _entries[index] = value;
        }

        private struct Entry
        {
            public readonly int Index;
            public readonly int Hash;
            public readonly TKey Key;
            public readonly bool IsNotDefault;
            public readonly int Version;
            public int Next;
            public TValue Value;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Entry(in int hash, in int index, in TKey key, in TValue value, int version)
            {
                Index = index;
                Hash = hash;
                Key = key;
                Value = value;
                Version = version;
                IsNotDefault = true;
                Next = -1;
            }

            public override string ToString() => $"{Key}: {Value}";
        }

        private struct Bucket
        {
            public readonly int FirstIndex;
            
            public int Head;
            public int Count;
            public int Version;

            public Bucket(in int version, in int firstIndex)
            {
                Version = version;
                FirstIndex = firstIndex;
                Head = firstIndex;
                Count = 0;
            }
            
            public override string ToString()
            {
                return $"v{Version.ToString()} - Head: {Head.ToString()}";
            }
        }
    }

    public readonly struct EntryIndex<TKey>
    {
        public readonly TKey Key;
        public readonly int Index;
        public readonly int Version;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EntryIndex(in int index, in TKey key, in int version)
        {
            Key = key;
            Version = version;
            Index = index;
        }

        public override string ToString() => $"{Key.ToString()}";
    }
}