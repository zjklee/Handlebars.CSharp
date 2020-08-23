using System;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Collections;

namespace HandlebarsDotNet.Compiler.Structure.Path
{
    /// <summary>
    /// Represents parts of single <see cref="PathSegment"/> separated with dots.
    /// </summary>
    public sealed class ChainSegment : IEquatable<ChainSegment>
    {
        private static readonly char[] TrimStart = {'@'};
        private static readonly LookupSlim<string, ChainSegment> Lookup = new LookupSlim<string, ChainSegment>();

        static ChainSegment() => Handlebars.Disposables.Add(new Disposer());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ChainSegment Create(string value) => Lookup.GetOrAdd(value, v => new ChainSegment(v));

        public static ChainSegment Index { get; } = Create(nameof(Index));
        public static ChainSegment First { get; } = Create(nameof(First));
        public static ChainSegment Last { get; } = Create(nameof(Last));
        public static ChainSegment Value { get; } = Create(nameof(Value));
        public static ChainSegment Key { get; } = Create(nameof(Key));
        public static ChainSegment Root { get; } = Create(nameof(Root));
        public static ChainSegment Parent { get; } = Create(nameof(Parent));
        
        private readonly object _lock = new object();

        private readonly int _hashCode;
        private readonly string _value;
        private UndefinedBindingResult _undefinedBindingResult;
        
        /// <summary>
        ///  
        /// </summary>
        private ChainSegment(string value)
        {
            var segmentValue = string.IsNullOrEmpty(value) ? "this" : value.TrimStart(TrimStart);
            var segmentTrimmedValue = TrimSquareBrackets(segmentValue);

            _value = segmentValue;
            IsThis = string.IsNullOrEmpty(value) || string.Equals(value, "this", StringComparison.OrdinalIgnoreCase);
            IsVariable = !string.IsNullOrEmpty(value) && value.StartsWith("@");
            TrimmedValue = segmentTrimmedValue;
            LowerInvariant = segmentTrimmedValue.ToLowerInvariant();
            
            IsValue = LowerInvariant == "value";
            IsKey = LowerInvariant == "key";

            _hashCode = GetHashCodeImpl();
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
        internal readonly bool IsValue;
        internal readonly bool IsKey;

        /// <summary>
        /// Returns string representation of current <see cref="ChainSegment"/>
        /// </summary>
        public override string ToString() => _value;

        /// <inheritdoc />
        public bool Equals(ChainSegment other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualsImpl(other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return EqualsImpl((ChainSegment) obj);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EqualsImpl(ChainSegment other)
        {
            return IsThis == other.IsThis
                   && LowerInvariant == other.LowerInvariant;
        }

        /// <inheritdoc />
        public override int GetHashCode() => _hashCode;

        private int GetHashCodeImpl()
        {
            unchecked
            {
                var hashCode = IsThis.GetHashCode();
                hashCode = (hashCode * 397) ^ (LowerInvariant.GetHashCode());
                return hashCode;
            }
        }

        /// <inheritdoc cref="Equals(HandlebarsDotNet.Compiler.Structure.Path.ChainSegment)"/>
        public static bool operator ==(ChainSegment a, ChainSegment b) => a.Equals(b);

        /// <inheritdoc cref="Equals(HandlebarsDotNet.Compiler.Structure.Path.ChainSegment)"/>
        public static bool operator !=(ChainSegment a, ChainSegment b) => !a.Equals(b);

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
        
        private sealed class Disposer : IDisposable
        {
            public void Dispose()
            {
                Lookup.Clear();
            }
        }
    }
}