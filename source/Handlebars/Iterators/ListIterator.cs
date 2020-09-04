using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class ListIterator<T, TValue> : Iterator<T>
        where T: class, IList<TValue>
    {
        public ListIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }
        
        public override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables,
            T target, Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new IteratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);

            var count = target.Count;

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;
            
            var index = 0;
            var lastIndex = count - 1;
            for (; index < count; index++)
            {
                var value = (object) target[index];
                var objectIndex = (object) index;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (index == lastIndex) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = objectIndex;
                
                blockParamsValues[blockParamsVariables[0]] = value;
                blockParamsValues[blockParamsVariables[1]] = objectIndex;
                
                iterator[ChainSegment.Value] = value;
                innerContext.Value = value;

                template(context, context.TextWriter, innerContext);
            }

            if (index == 0)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
    
    internal sealed class ListIterator<T> : Iterator<T>
        where T: class, IList
    {
        public ListIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }
        
        public override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables,
            T target, Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new IteratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);

            var count = target.Count;

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;
            
            var index = 0;
            var lastIndex = count - 1;
            for (; index < count; index++)
            {
                var value = target[index];
                var objectIndex = (object) index;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (index == lastIndex) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = objectIndex;
                
                blockParamsValues[blockParamsVariables[0]] = value;
                blockParamsValues[blockParamsVariables[1]] = objectIndex;
                
                iterator[ChainSegment.Value] = value;
                innerContext.Value = value;

                template(context, context.TextWriter, innerContext);
            }

            if (index == 0)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}