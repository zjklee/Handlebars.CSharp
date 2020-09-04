using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet.ValueProviders
{
    public class BlockParamsVariables
    {
        private readonly ChainSegment[] _indexes;

        internal BlockParamsVariables(TemplateContext templateContext, string[] variables)
        {
            _indexes = GetIndexes(templateContext, variables);
        }
        
        public ChainSegment this[in int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(_indexes.Length == 0 || index < 0 || index >= _indexes.Length) return null;
                return _indexes[index];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ChainSegment[] GetIndexes(TemplateContext templateContext, string[] variables)
        {
            if (templateContext == null || variables == null || variables.Length == 0)
            {
                return ArrayEx.Empty<ChainSegment>();
            }
            
            var vars = new ChainSegment[variables.Length];
            for (int index = 0; index < vars.Length; index++)
            {
                vars[index] = ChainSegment.Create(templateContext, variables[index]);
            }

            return vars;
        }
    }
    
    public readonly ref struct BlockParamsValues
    {
        private readonly Dictionary<ChainSegment, object> _values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockParamsValues(BindingContext context)
        {
            _values = context?.BlockParams;
        }
        
        public object this[in ChainSegment index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if(_values == null || ReferenceEquals(index, null)) return;
                _values[index] = value;
            }
        }
    }
}