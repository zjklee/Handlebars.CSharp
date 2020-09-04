using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    public readonly ref struct IteratorValueProvider
    {
        private readonly Dictionary<ChainSegment, object> _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IteratorValueProvider(BindingContext bindingContext) : this()
        {
            _data = bindingContext.DataObject;
        }
        
        public object this[in ChainSegment index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if(ReferenceEquals(index, null)) return;
                _data[index] = value;
            }
        }
    }
}