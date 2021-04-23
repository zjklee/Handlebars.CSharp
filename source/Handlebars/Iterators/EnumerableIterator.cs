using System.Collections;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.PathStructure;
using HandlebarsDotNet.Runtime;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    public sealed class EnumerableIterator<T> : IIterator
        where T: class, IEnumerable
    {
        public void Iterate(
            in EncodedTextWriter writer,
            BindingContext context,
            ChainSegment[] blockParamsVariables,
            in Arguments arguments,
            object input,
            TemplateDelegate template,
            TemplateDelegate ifEmpty)
        {
            using var innerContext = context.CreateFrame();
            var iterator = new IteratorValues(innerContext);
            var blockParamsValues = new BlockParamsValues(innerContext, blockParamsVariables);

            blockParamsValues.CreateProperty(0, out var _0);
            blockParamsValues.CreateProperty(1, out var _1);

            var target = (T) input;
            var enumerator = ExtendedEnumerator<object>.Create(target.GetEnumerator());

            iterator.First = BoxedValues.True;
            iterator.Last = BoxedValues.False;

            int index = 0;
            var indexOffset = IIterator.Helpers.GetIndexOffset(context, arguments);
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                
                var value = current.Value;
                var indexObject = BoxedValues.Int(index + indexOffset);
                
                if (index == 1) iterator.First = BoxedValues.False;
                if (current.IsLast) iterator.Last = BoxedValues.True;
                
                iterator.Index = indexObject;
                
                blockParamsValues[_0] = value;
                blockParamsValues[_1] = indexObject;
                
                iterator.Value = value;
                innerContext.Value = value;

                template(writer, innerContext);

                ++index;
            }
            
            if (index == 0)
            {
                innerContext.Value = context.Value;
                ifEmpty(writer, innerContext);
            }
        }
    }
}