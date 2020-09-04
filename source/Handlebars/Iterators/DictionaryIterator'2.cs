using System;
using System.Collections.Generic;
using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class DictionaryIterator<TDictionary, TKey, TValue> : Iterator<TDictionary>
        where TDictionary : class, IDictionary<TKey, TValue>
    {
        public DictionaryIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }
        
        public override void Iterate(
            BindingContext context, 
            BlockParamsVariables blockParamsVariables,
            TDictionary target, 
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty
        )
        {
            using var innerContext = context.CreateFrame();
            var iterator = new ObjectEnumeratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);
            
            using var enumerator = target.GetEnumerator();

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;

            var index = 0;
            int lastIndex = target.Count - 1;
            while (enumerator.MoveNext())
            {
                var key = (object) enumerator.Current.Key;
                var value = (object) enumerator.Current.Value;
                
                iterator[ChainSegment.Key] = key;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (index == lastIndex) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = index;

                blockParamsValues[blockParamsVariables[0]] = value;
                blockParamsValues[blockParamsVariables[1]] = key;
                
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