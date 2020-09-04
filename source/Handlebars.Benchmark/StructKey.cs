using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using HandlebarsDotNet.Collections;

namespace HandlebarsNet.Benchmark
{
    public struct StructObjectComparer : IEqualityComparer<object>
    {
        public bool Equals(object x, object y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => obj.GetHashCode();
    }
    
    public class ObjectComparer : IEqualityComparer<object>
    {
        public bool Equals(object x, object y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => obj.GetHashCode();
    }
    
    public class StructKeyDictionary
    {
        private Dictionary<object, int> _default;
        private FixedSizeDictionary<object, int, StructObjectComparer> _fixedSizeDictionary;

        private EntryIndex<object> _entryIndex;
        private object _knownObject;
        private object _knownMissingObject;

        [GlobalSetup]
        public void Setup()
        {
            _default = new Dictionary<object, int>(256, new ObjectComparer());
            _fixedSizeDictionary = new FixedSizeDictionary<object, int, StructObjectComparer>(256, new StructObjectComparer());

            _knownObject = new object();
            _knownMissingObject = new object();
            
            _default.Add(_knownObject, 42);
            _fixedSizeDictionary.AddOrReplace(_knownObject, 42, out _entryIndex);
        }
        
        [GlobalCleanup]
        public void Cleanup()
        {
            _default?.Clear();
            _fixedSizeDictionary?.Reset();
        }
        
        [Benchmark]
        public void DefaultAdd()
        {
            _default.Add(new object(), 42);
        }
        
        [Benchmark]
        public void FixedSizeDictionaryAddOrReplace()
        {
            _fixedSizeDictionary.AddOrReplace(new object(), 42, out _);
        }

        [Benchmark]
        public void FixedSizeDictionaryAdd()
        {
            _fixedSizeDictionary.Add(new object(), 42, out _);
        }

        [Benchmark]
        public bool DefaultContains()
        {
            return _default.ContainsKey(_knownObject);
        }
        
        [Benchmark]
        public void DefaultReplace()
        {
            _default[_knownObject] = 43;
        }
        
        [Benchmark]
        public void DefaultTryGet()
        {
            _default.TryGetValue(_knownObject, out _);
        }
        
        [Benchmark]
        public void DefaultGet()
        {
            _ = _default[_knownObject];
        }
        
        [Benchmark]
        public void FixedSizeDictionaryReplace()
        {
            _fixedSizeDictionary[_entryIndex] = 43;
        }

        [Benchmark]
        public void FixedSizeDictionaryTryGetByIndex()
        {
            _fixedSizeDictionary.TryGetValue(_entryIndex, out _);
        }

        [Benchmark]
        public void FixedSizeDictionaryGetByIndex()
        {
            _ = _fixedSizeDictionary[_entryIndex];
        }

        [Benchmark]
        public bool FixedSizeDictionaryContainsByIndex()
        {
            return _fixedSizeDictionary.ContainsKey(_entryIndex);
        }

        [Benchmark]
        public bool DefaultContainsNotExists()
        {
            return _default.ContainsKey(new object());
        }

        [Benchmark]
        public bool FixedSizeDictionaryNotExists()
        {
            return _fixedSizeDictionary.ContainsKey(new object());
        }
    }
}