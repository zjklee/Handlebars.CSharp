using System;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    internal sealed class ObjectEnumeratorValueProvider : IValueProvider, IDisposable
    {
        private ICompiledHandlebarsConfiguration _configuration;

        private static readonly InternalObjectPool<ObjectEnumeratorValueProvider> Pool = new InternalObjectPool<ObjectEnumeratorValueProvider>(new Policy());
        
        public static ObjectEnumeratorValueProvider Create(ICompiledHandlebarsConfiguration configuration)
        {
            var provider = Pool.Get();
            provider._configuration = configuration;
            return provider;
        }

        public Ref<ChainSegment> Key { get; } = new Ref<ChainSegment>(null);
        
        public Ref<int> Index { get; } = new Ref<int>(0);

        public Ref<bool> First { get; } = new Ref<bool>(true);

        public Ref<bool> Last { get; } = new Ref<bool>(false);

        public void Attach(BindingContext bindingContext)
        {
            bindingContext.ContextDataObject[ChainSegment.Index] = Index;
            bindingContext.ContextDataObject[ChainSegment.First] = First;
            bindingContext.ContextDataObject[ChainSegment.Key] = Key;

            if (!_configuration.Compatibility.SupportLastInObjectIterations)
            {
                bindingContext.ContextDataObject[ChainSegment.Last] = new UndefinedBindingResult("@last", _configuration).AsRef();
            }
            else
            {
                bindingContext.ContextDataObject[ChainSegment.Last] = Last;
            }
        }

        public void Dispose() => Pool.Return(this);

        private class Policy : IInternalObjectPoolPolicy<ObjectEnumeratorValueProvider>
        {
            ObjectEnumeratorValueProvider IInternalObjectPoolPolicy<ObjectEnumeratorValueProvider>.Create()
            {
                return new ObjectEnumeratorValueProvider();
            }

            bool IInternalObjectPoolPolicy<ObjectEnumeratorValueProvider>.Return(ObjectEnumeratorValueProvider item)
            {
                item.First.Value = true;
                item.Last.Value = false;
                item.Index.Value = 0;
                item.Key.Value = null;
                item._configuration = null;

                return true;
            }
        }
    }
}