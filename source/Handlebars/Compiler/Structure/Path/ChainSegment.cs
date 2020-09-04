using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HandlebarsDotNet;

namespace HandlebarsDotNet.Compiler.Structure.Path
{
    /// <summary>
    /// Represents parts of single <see cref="PathSegment"/> separated with dots.
    /// </summary>
    public sealed class ChainSegment : IEquatable<ChainSegment>, IEquatable<string>
    {
        private static readonly char[] TrimStart = {'@'};
        public static IEqualityComparer<ChainSegment> DefaultEqualityComparer { get; } = new DefaultEqualityComparerImpl();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ChainSegment Create(TemplateContext context, string value) => context.CreateChainSegment(value);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ChainSegment Create(TemplateContext context, object value)
        {
            if (value is ChainSegment segment) return segment;
            return Create(context, value as string ?? value.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ChainSegment Create(string value, bool isVariable = true)
        {
            var variable = isVariable ? $"@{value}" : value;
            
            return Create(TemplateContext.Shared, variable);
        }

        public static ChainSegment Index { get; } = Create(nameof(Index));
        public static ChainSegment First { get; } = Create(nameof(First));
        public static ChainSegment Last { get; } = Create(nameof(Last));
        public static ChainSegment Value { get; } = Create(nameof(Value));
        public static ChainSegment Key { get; } = Create(nameof(Key));
        public static ChainSegment Root { get; } = Create(nameof(Root));
        public static ChainSegment Parent { get; } = Create(nameof(Parent));
        public static ChainSegment This { get; } = Create(nameof(This), false);
        
        private readonly object _lock = new object();

        public readonly int HashCode;
        private readonly string _value;
        private UndefinedBindingResult _undefinedBindingResult;
        
        /// <summary>
        ///  
        /// </summary>
        internal ChainSegment(in string value)
        {
            var segmentValue = string.IsNullOrEmpty(value) ? "this" : value.TrimStart(TrimStart);
            var segmentTrimmedValue = TrimSquareBrackets(segmentValue);

            _value = segmentValue;
            IsThis = string.IsNullOrEmpty(value) || string.Equals(value, "this", StringComparison.OrdinalIgnoreCase);
            IsVariable = !string.IsNullOrEmpty(value) && value.StartsWith("@");
            TrimmedValue = segmentTrimmedValue;
            LowerInvariant = segmentTrimmedValue.ToLowerInvariant();
            
            HashCode = GetHashCodeImpl();
        }

        /// <summary>
        /// Value with trimmed '[' and ']'
        /// </summary>
        public readonly string TrimmedValue;
        
        /// <summary>
        /// Indicates whether <see cref="ChainSegment"/> is part of <c>@</c> variable
        /// </summary>
        public readonly bool IsVariable;
        
        /// <summary>
        /// Indicates whether <see cref="ChainSegment"/> is <c>this</c> or <c>.</c>
        /// </summary>
        public readonly bool IsThis;

        internal readonly string LowerInvariant;

        /// <summary>
        /// Returns string representation of current <see cref="ChainSegment"/>
        /// </summary>
        public override string ToString() => _value;

        /// <inheritdoc />
        public bool Equals(ChainSegment other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DefaultEqualityComparer.Equals(this, other);
        }

        public bool Equals(string other)
        {
            return LowerInvariant == other?.ToLowerInvariant();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is ChainSegment other)) return false;
            return DefaultEqualityComparer.Equals(this, other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode;

        private int GetHashCodeImpl()
        {
            unchecked
            {
                var hashCode = IsThis.GetHashCode();
                //hashCode = (hashCode * 397) ^ IsThis.GetHashCode();
                hashCode = (hashCode * 397) ^ (LowerInvariant?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
        
        /// <inheritdoc cref="Equals(ChainSegment)"/>
        public static bool operator ==(ChainSegment a, ChainSegment b) => DefaultEqualityComparer.Equals(a, b);

        /// <inheritdoc cref="Equals(ChainSegment)"/>
        public static bool operator !=(ChainSegment a, ChainSegment b) => !DefaultEqualityComparer.Equals(a, b);

        /// <inheritdoc cref="ToString"/>
        public static implicit operator string(ChainSegment segment) => segment._value;
        
        /// <summary>
        /// 
        /// </summary>
        
        public static implicit operator ChainSegment(string segment) => Create(segment);

        private static string TrimSquareBrackets(string key)
        {
            //Only trim a single layer of brackets.
            if (key.StartsWith("[") && key.EndsWith("]"))
            {
                return key.Substring(1, key.Length - 2);
            }

            return key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UndefinedBindingResult GetUndefinedBindingResult(ICompiledHandlebarsConfiguration configuration)
        {
            if (_undefinedBindingResult != null) return _undefinedBindingResult;
            lock (_lock)
            {
                return _undefinedBindingResult ??= new UndefinedBindingResult(this, configuration);
            }
        }
        
        private sealed class DefaultEqualityComparerImpl : IEqualityComparer<ChainSegment>
        {
            public bool Equals(ChainSegment x, ChainSegment y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                
                return x.HashCode == y.HashCode && /*x.IsVariable == y.IsVariable &&*/ x.IsThis == y.IsThis && x.LowerInvariant == y.LowerInvariant;
            }

            public int GetHashCode(ChainSegment obj) => obj.HashCode;
        }
    }
}