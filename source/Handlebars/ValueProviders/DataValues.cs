using System.Collections.Generic;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    public sealed class DataValues : IValueProvider
    {
        private Dictionary<ChainSegment, Ref> _data;
        
        internal DataValues(Dictionary<ChainSegment, Ref> data)
        {
            _data = data;
        }

        public Ref this[ChainSegment segment]
        {
            get
            {
                if (_data.TryGetValue(segment, out var @ref)) return @ref;
                return null;
            }
            set
            {
                if (_data.TryGetValue(segment, out var @ref))
                {
                    @ref.SetValue(value);
                    return;
                }
                
                _data.Add(segment, value);
            }
        }

        void IValueProvider.Attach(BindingContext bindingContext)
        {
            _data = bindingContext.ContextDataObject;
        }
    }
}