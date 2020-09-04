using System;
using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class ObjectIterator : Iterator
    {
        public ObjectIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }

        public override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables,
            object target, Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new ObjectEnumeratorValueProvider(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext);
            
            var properties = (ChainSegment[]) Descriptor.GetProperties(Descriptor, target);
            
            iterator[ChainSegment.First] = BoxedValues.True;
            iterator[ChainSegment.Last] = BoxedValues.False;

            var index = 0;
            int lastIndex = properties.Length - 1;
            for (; index < properties.Length; index++)
            {
                var iteratorKey = properties[index];
                iterator[ChainSegment.Key] = iteratorKey;
                
                if (index == 1) iterator[ChainSegment.First] = BoxedValues.False;
                if (index == lastIndex) iterator[ChainSegment.Last] = BoxedValues.True;
                
                iterator[ChainSegment.Index] = index;
                
                var resolvedValue = Descriptor.AccessMember(target, iteratorKey);
                
                blockParamsValues[blockParamsVariables[0]] = resolvedValue;
                blockParamsValues[blockParamsVariables[1]] = iteratorKey;
                
                iterator[ChainSegment.Value] = resolvedValue;
                innerContext.Value = resolvedValue;

                template(context, context.TextWriter, innerContext);
            }
            
            if (index == 0)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}