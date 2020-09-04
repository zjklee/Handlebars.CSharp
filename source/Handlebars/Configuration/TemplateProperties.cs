using System;
using System.Collections.Generic;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet
{
    public class TemplateProperties
    {
        public string TemplatePath { get; internal set; }
    }

    public class TemplateContext
    {
        private static readonly Lazy<TemplateContext> Self = new Lazy<TemplateContext>(() => new TemplateContext(true));
        internal static TemplateContext Shared => Self.Value;
        
        private readonly bool _isShared;
        private readonly LookupSlim<string, DeferredValue<string, ChainSegment>> _chainSegmentsCache;
        internal readonly LookupSlim<Type, DeferredValue<ValuePair<Type, ICompiledHandlebarsConfiguration>, TypeDescriptor>> TypeDescriptors 
            = new LookupSlim<Type, DeferredValue<ValuePair<Type, ICompiledHandlebarsConfiguration>, TypeDescriptor>>();

        public TemplateContext(bool isShared = false)
        {
            _isShared = isShared;
            if(isShared)
            {
                PathInfoStore = new PathInfoStore(this);
            }
            else
            {
                PathInfoStore = new CombiningPathInfoStore(Shared.PathInfoStore, new PathInfoStore(this));    
            }
            
            _chainSegmentsCache = new LookupSlim<string, DeferredValue<string, ChainSegment>>(new LookupSlimIgnoreCasePolicy());
        }
        
        public IPathInfoStore PathInfoStore { get; }

        internal ChainSegment CreateChainSegment(string segment)
        {
            if (!_isShared && Shared._chainSegmentsCache.TryGetValue(segment, out var value))
            {
                return value.Value;
            }
            
            if (_chainSegmentsCache.TryGetValue(segment, out value))
            {
                return value.Value;
            }

            return _chainSegmentsCache.GetOrAdd(segment, s =>
            {
                return new DeferredValue<string, ChainSegment>(s, c => new ChainSegment(c));
            }).Value;
        }

        private class LookupSlimIgnoreCasePolicy : IInternalObjectPoolPolicy<Dictionary<string, DeferredValue<string, ChainSegment>>>
        {
            public Dictionary<string, DeferredValue<string, ChainSegment>> Create()
            {
                return new Dictionary<string, DeferredValue<string, ChainSegment>>(StringComparer.OrdinalIgnoreCase);
            }

            public bool Return(Dictionary<string, DeferredValue<string, ChainSegment>> item)
            {
                item.Clear();
                return true;
            }
        }
        
        private class CombiningPathInfoStore : IPathInfoStore
        {
            private readonly IPathInfoStore _first;
            private readonly IPathInfoStore _second;

            public CombiningPathInfoStore(IPathInfoStore first, IPathInfoStore second)
            {
                _first = first;
                _second = second;
            }
            
            public PathInfo GetOrAdd(string path)
            {
                if (_first.TryGetValue(path, out var pathInfo) || _second.TryGetValue(path, out pathInfo))
                {
                    return pathInfo;
                }

                return _second.GetOrAdd(path);
            }

            public bool TryGetValue(string path, out PathInfo pathInfo)
            {
                return _first.TryGetValue(path, out pathInfo) || _second.TryGetValue(path, out pathInfo);
            }
        }
    }
    
    public readonly struct ValuePair<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
            
        public ValuePair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}