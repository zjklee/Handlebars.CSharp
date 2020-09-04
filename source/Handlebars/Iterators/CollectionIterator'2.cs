using System;
using System.Collections.Generic;
using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class CollectionIterator<T, TValue> : Iterator<T>
        where T: class, ICollection<TValue>
    {
        public CollectionIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }
        
        public override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables,
            T target, Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new IteratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);
            
            using var enumerator = target.GetEnumerator();

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;

            int index = 0;
            var lastIndex = target.Count - 1;
            while (enumerator.MoveNext())
            {
                var value = (object) enumerator.Current;
                var indexObject = (object) index;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (index == lastIndex) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = indexObject;
                
                blockParamsValues[blockParamsVariables[0]] = value;
                blockParamsValues[blockParamsVariables[1]] = indexObject;
                
                iterator[ChainSegment.Value] = value;
                innerContext.Value = value;

                template(context, context.TextWriter, innerContext);

                index++;
            }
            
            if (index == 0)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}