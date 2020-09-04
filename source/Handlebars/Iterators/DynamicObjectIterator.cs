using System;
using System.IO;
using System.Linq;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class DynamicObjectIterator : Iterator
    {
        public DynamicObjectIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }

        public override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables,
            object target, Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new ObjectEnumeratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);
            
            var properties = Descriptor.GetProperties(Descriptor, target).Cast<ChainSegment>();
            var enumerator = new ExtendedEnumerator<ChainSegment>(properties.GetEnumerator());
            var enumerated = false;

            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;

            int index = 0;
            while (enumerator.MoveNext())
            {
                enumerated = true;
                var current = enumerator.Current;
                
                var iteratorKey = current.Value;
                iterator[ChainSegment.Key] = iteratorKey;
                
                if (current.IsFirst) iterator[ChainSegment.First] = BoxedValues.True;
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (current.IsLast) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = index++;
                
                var resolvedValue = Descriptor.AccessMember(target, iteratorKey);
                
                blockParamsValues[blockParamsVariables[0]] = resolvedValue;
                blockParamsValues[blockParamsVariables[1]] = iteratorKey;
                
                iterator[ChainSegment.Value] = resolvedValue;
                innerContext.Value = resolvedValue;

                template(context, context.TextWriter, innerContext);
            }
            
            if (!enumerated)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}