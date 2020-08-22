using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    public sealed class BlockParamsValues : IValueProvider, IDisposable
    {
        private static readonly InternalObjectPool<BlockParamsValues> Pool = new InternalObjectPool<BlockParamsValues>(new Policy());
        
        public static BlockParamsValues Empty { get; } = new BlockParamsValues();
        
        private ChainSegment[] _variables;

        private Dictionary<ChainSegment, Ref> _values;

        public static BlockParamsValues Create(ChainSegment[] variables)
        {
            var item = Pool.Get();
            item._variables = variables;
            return item;
        }
        
        private BlockParamsValues()
        {
        }
        
        public object this[int index]
        {
            set
            {
                var variable = GetVariable(index);
                if(ReferenceEquals(variable, null)) return;
                if (_values.TryGetValue(variable, out var @ref))
                {
                    @ref.SetValue(value);
                }

                if (value is Ref refValue)
                {
                    _values.Add(variable, refValue);
                    return;
                }
                
                _values.Add(variable, value.AsRef());
            }
        }
        
        void IValueProvider.Attach(BindingContext bindingContext)
        {
            _values = bindingContext.BlockParamsObject;
            bindingContext.BlockParams = this;
        }

        public void Dispose()
        {
            if(ReferenceEquals(this, Empty)) return;
            Pool.Return(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChainSegment GetVariable(int index)
        {
            if(_variables == null || _variables.Length == 0) return null;
            if(index >= _variables.Length || index < 0) return null;
            return _variables[index];
        }

        private class Policy : IInternalObjectPoolPolicy<BlockParamsValues>
        {
            public BlockParamsValues Create() => new BlockParamsValues();

            public bool Return(BlockParamsValues item)
            {
                item._values = null;
                item._variables = null;
                
                return true;
            }
        }
    }
}