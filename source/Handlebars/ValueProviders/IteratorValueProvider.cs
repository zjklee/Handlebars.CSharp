using System;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    internal sealed class IteratorValueProvider : IValueProvider, IDisposable
    {
        private static readonly InternalObjectPool<IteratorValueProvider> Pool = new InternalObjectPool<IteratorValueProvider>(new Policy());

        public static IteratorValueProvider Create() => Pool.Get();

        public Ref<int> Index { get; } = new Ref<int>(0);

        public Ref<bool> First { get; } = new Ref<bool>(true);

        public Ref<bool> Last { get; } = new Ref<bool>(false);
        
        public void Attach(BindingContext bindingContext)
        {
            bindingContext.ContextDataObject[ChainSegment.Index] = Index;
            bindingContext.ContextDataObject[ChainSegment.First] = First;
            bindingContext.ContextDataObject[ChainSegment.Last] = Last;
        }

        public void Dispose() => Pool.Return(this);

        private class Policy : IInternalObjectPoolPolicy<IteratorValueProvider>
        {
            IteratorValueProvider IInternalObjectPoolPolicy<IteratorValueProvider>.Create() => new IteratorValueProvider();

            bool IInternalObjectPoolPolicy<IteratorValueProvider>.Return(IteratorValueProvider item)
            {
                item.First.Value = true;
                item.Last.Value = false;
                item.Index.Value = 0;

                return true;
            }
        }
    }
}