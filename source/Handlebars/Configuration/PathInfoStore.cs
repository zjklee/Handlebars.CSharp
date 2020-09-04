using System;
using System.Collections.Generic;
using System.Linq;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet
{
    /// <summary>
    /// Provides access to path expressions in the template
    /// </summary>
    public interface IPathInfoStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        PathInfo GetOrAdd(string path);

        bool TryGetValue(string path, out PathInfo pathInfo);
    }
    
    internal class PathInfoStore : IPathInfoStore
    {
        private readonly TemplateContext _templateContext;
        private readonly LookupSlim<string, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>> _paths 
            = new LookupSlim<string, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>>(new LookupSlimIgnoreCasePolicy());

        private static readonly Func<string, PathInfoStore, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>> ValueFactory = (p, self) =>
        {
            return new DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>(
                new ValuePair<string, PathInfoStore>(p, self), deps => deps.Item2.GetPathInfo(deps.Item1)
            );
        };

        public PathInfoStore(TemplateContext templateContext)
        {
            _templateContext = templateContext;
        }

        public bool TryGetValue(string path, out PathInfo pathInfo)
        {
            if (_paths.TryGetValue(path, out var deferredValue))
            {
                pathInfo = deferredValue.Value;
                return true;
            }

            pathInfo = null;
            return false;
        }
        
        public PathInfo GetOrAdd(string path)
        {
            if (_paths.TryGetValue(path, out var deferredValue)) return deferredValue.Value;
            
            deferredValue = _paths.GetOrAdd(path, ValueFactory, this);

            var pathInfo = deferredValue.Value;
            
            var trimmedPath = pathInfo.TrimmedPath;
            if ((pathInfo.IsBlockHelper || pathInfo.IsInversion) && !_paths.ContainsKey(trimmedPath))
            {
                _paths.GetOrAdd(trimmedPath, ValueFactory, this);
            }

            return deferredValue.Value;
        }

        private PathInfo GetPathInfo(string path)
        {
            if (path == "null")
                return new PathInfo(false, path, false, null);

            var originalPath = path;

            var isValidHelperLiteral = true;
            var isVariable = path.StartsWith("@");
            var isInversion = path.StartsWith("^");
            var isBlockHelper = path.StartsWith("#");
            if (isVariable || isBlockHelper || isInversion)
            {
                isValidHelperLiteral = isBlockHelper || isInversion;
                path = path.Substring(1);
            }

            var segments = new List<PathSegment>();
            var pathParts = path.Split('/');
            if (pathParts.Length > 1) isValidHelperLiteral = false;
            foreach (var segment in pathParts)
            {
                if (segment == "..")
                {
                    isValidHelperLiteral = false;
                    segments.Add(new PathSegment(segment, ArrayEx.Empty<ChainSegment>()));
                    continue;
                }

                if (segment == ".")
                {
                    isValidHelperLiteral = false;
                    segments.Add(new PathSegment(segment, ArrayEx.Empty<ChainSegment>()));
                    continue;
                }

                var segmentString = isVariable ? "@" + segment : segment;
                var chainSegments = GetPathChain(segmentString).ToArray();
                if (chainSegments.Length > 1) isValidHelperLiteral = false;

                segments.Add(new PathSegment(segmentString, chainSegments));
            }

            return new PathInfo(true, originalPath, isValidHelperLiteral, segments.ToArray());
        }

        private IEnumerable<ChainSegment> GetPathChain(string segmentString)
        {
            var insideEscapeBlock = false;
            var pathChainParts = segmentString.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            if (pathChainParts.Length == 0 && segmentString == ".") return new[] {ChainSegment.This};

            var pathChain = pathChainParts.Aggregate(new List<ChainSegment>(), (list, next) =>
            {
                if (insideEscapeBlock)
                {
                    if (next.EndsWith("]"))
                    {
                        insideEscapeBlock = false;
                    }

                    list[list.Count - 1] = ChainSegment.Create(_templateContext, $"{list[list.Count - 1]}.{next}");
                    return list;
                }

                if (next.StartsWith("["))
                {
                    insideEscapeBlock = true;
                }

                if (next.EndsWith("]"))
                {
                    insideEscapeBlock = false;
                }

                list.Add(ChainSegment.Create(_templateContext, next));
                return list;
            });

            return pathChain;
        }
        
        private class LookupSlimIgnoreCasePolicy : IInternalObjectPoolPolicy<Dictionary<string, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>>>
        {
            public Dictionary<string, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>> Create()
            {
                return new Dictionary<string, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>>(StringComparer.OrdinalIgnoreCase);
            }

            public bool Return(Dictionary<string, DeferredValue<ValuePair<string, PathInfoStore>, PathInfo>> item)
            {
                item.Clear();
                return true;
            }
        }
    }
}