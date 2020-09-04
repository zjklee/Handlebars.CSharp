using System;
using System.IO;
using System.Text.Json;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Iterators;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.Structure.Path;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Extension.Json.Iterators
{
    internal class JsonObjectIterator : Iterator<JsonElement>
    {
        public JsonObjectIterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }


        public override void Iterate(
            BindingContext context, 
            ChainSegment[] blockParamsVariables, 
            JsonElement target, 
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            JsonElement.ObjectEnumerator objectEnumerator = target.EnumerateObject();
            
            using var innerContext = context.CreateFrame();
            var iterator = new ObjectEnumeratorValueProvider<string, JsonElement>(innerContext);
            var blockParamsValues = new BlockParamsValues(blockParamsVariables, innerContext);
            
            using var enumerator = new ExtendedEnumerator<JsonElement.ObjectEnumerator, JsonProperty>(objectEnumerator);

            blockParamsValues[0] = iterator.CurrentValue;
            blockParamsValues[1] = iterator.Key;
            
            var index = 0;
            var enumerated = false;
            
            while (enumerator.MoveNext())
            {
                enumerated = true;
                var current = enumerator.Current;
                
                iterator.First.Self = current.IsFirst;
                iterator.Last.Self = current.IsLast;
                iterator.Index.Self = index++;

                iterator.CurrentValue.Self = current.Value.Value;
                innerContext.Value = current.Value.Value;

                template(context, context.TextWriter, innerContext);
            }

            if (!enumerated)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}