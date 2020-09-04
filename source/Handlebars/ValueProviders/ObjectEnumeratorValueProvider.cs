using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    public readonly ref struct ObjectEnumeratorValueProvider
    {
        private readonly Dictionary<ChainSegment, object> _data;
        private readonly bool _supportLastInObjectIterations;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectEnumeratorValueProvider(BindingContext bindingContext) : this()
        {
            _data = bindingContext.DataObject;
            _supportLastInObjectIterations = bindingContext.Configuration.Compatibility.SupportLastInObjectIterations;
            if (!_supportLastInObjectIterations)
            {
                _data[ChainSegment.Last] = ChainSegment.Last.GetUndefinedBindingResult(bindingContext.Configuration);
            }
        }
        
        public object this[in ChainSegment index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if(ReferenceEquals(index, null)) return;
                if(!_supportLastInObjectIterations && index == ChainSegment.Last) return;
                
                _data[index] = value;
            }
        }
    }
}