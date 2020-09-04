using System;
using System.Collections.Generic;
using System.IO;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class EnumerableIterator<T, TValue> : Iterator<T> 
        where T : class, IEnumerable<TValue>
    {
        public EnumerableIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }
        
        public override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables,
            T target, Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new IteratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);
            
            var enumerator = new ExtendedEnumerator<TValue>(target.GetEnumerator());
            var enumerated = false;

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;

            int index = 0;
            while (enumerator.MoveNext())
            {
                enumerated = true;
                var current = enumerator.Current;
                
                var value = (object) current.Value;
                var indexObject = (object) index;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (current.IsLast) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = indexObject;
                
                blockParamsValues[blockParamsVariables[0]] = value;
                blockParamsValues[blockParamsVariables[1]] = indexObject;
                
                iterator[ChainSegment.Value] = value;
                innerContext.Value = value;

                template(context, context.TextWriter, innerContext);

                index++;
            }
            
            if (!enumerated)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}