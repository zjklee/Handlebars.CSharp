using System;
using System.Collections;
using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class DictionaryIterator<TDictionary> : Iterator<TDictionary>
        where TDictionary : class, IDictionary
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
            
            var properties = target.Keys;
            var enumerator = properties.GetEnumerator();

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;

            var index = 0;
            int lastIndex = properties.Count - 1;
            while (enumerator.MoveNext())
            {
                var iteratorKey = enumerator.Current;
                iterator[ChainSegment.Key] = iteratorKey;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (index == lastIndex) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = index;
                
                var resolvedValue = target[iteratorKey!];
                
                blockParamsValues[blockParamsVariables[0]] = resolvedValue;
                blockParamsValues[blockParamsVariables[1]] = iteratorKey;
                
                iterator[ChainSegment.Value] = resolvedValue;
                innerContext.Value = resolvedValue;

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