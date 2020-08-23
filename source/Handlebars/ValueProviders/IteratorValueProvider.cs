using System;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    internal readonly struct IteratorValueProvider : IDisposable
    {
        public static IteratorValueProvider Create(BindingContext bindingContext)
        {
            return new IteratorValueProvider(
                bindingContext, 
                RefPool<int>.Shared.Create(0), 
                RefPool<bool>.Shared.Create(true), 
                RefPool<bool>.Shared.Create(false)
            );
        }
        
        private IteratorValueProvider(BindingContext bindingContext, 
            ReusableRef<int> index,
            ReusableRef<bool> first,
            ReusableRef<bool> last) : this()
        {
            Index = index;
            First = first;
            Last = last;
            
            bindingContext.ContextDataObject[ChainSegment.Index] = Index;
            bindingContext.ContextDataObject[ChainSegment.First] = First;
            bindingContext.ContextDataObject[ChainSegment.Last] = Last;
        }

        public readonly ReusableRef<int> Index;

        public readonly ReusableRef<bool> First;

        public readonly ReusableRef<bool> Last;
        
        public void Dispose()
        { 
            RefPool<int>.Shared.Return(Index);
            RefPool<bool>.Shared.Return(First);
            RefPool<bool>.Shared.Return(Last);
        }
    }
}