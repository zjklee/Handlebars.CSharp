using System;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.ValueProviders
{
    internal readonly struct ObjectEnumeratorValueProvider : IDisposable
    {
        private static readonly Ref<object> UndefinedLast = new Ref<object>(null);

        public static ObjectEnumeratorValueProvider Create(BindingContext bindingContext)
        {
            return new ObjectEnumeratorValueProvider(
                bindingContext,
                RefPool<object>.Shared.Create((object) null),
                RefPool<int>.Shared.Create(0),
                RefPool<bool>.Shared.Create(true),
                RefPool<bool>.Shared.Create(false)
                );
        }

        private ObjectEnumeratorValueProvider(
            BindingContext bindingContext, 
            ReusableRef<object> key, 
            ReusableRef<int> index, 
            ReusableRef<bool> first, 
            ReusableRef<bool> last) : this()
        {
            Key = key;
            Index = index;
            First = first;
            Last = last;

            var configuration = bindingContext.Configuration;
            UndefinedLast.SetValue(ChainSegment.Last.GetUndefinedBindingResult(configuration));
            
            bindingContext.ContextDataObject[ChainSegment.Index] = Index;
            bindingContext.ContextDataObject[ChainSegment.First] = First;
            bindingContext.ContextDataObject[ChainSegment.Key] = Key;

            if (!configuration.Compatibility.SupportLastInObjectIterations)
            {
                bindingContext.ContextDataObject[ChainSegment.Last] = UndefinedLast;
            }
            else
            {
                bindingContext.ContextDataObject[ChainSegment.Last] = Last;
            }
        }

        public readonly ReusableRef<object> Key;

        public readonly ReusableRef<int> Index;

        public readonly ReusableRef<bool> First;

        public readonly ReusableRef<bool> Last;

        public void Dispose()
        {
            RefPool<object>.Shared.Return(Key);
            RefPool<int>.Shared.Return(Index);
            RefPool<bool>.Shared.Return(First);
            RefPool<bool>.Shared.Return(Last);
        }
    }
}