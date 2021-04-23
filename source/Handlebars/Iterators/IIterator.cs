using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.PathStructure;

namespace HandlebarsDotNet.Iterators
{
    public interface IIterator
    {
        void Iterate(
            in EncodedTextWriter writer,
            BindingContext context,
            ChainSegment[] blockParamsVariables,
            in Arguments arguments,
            object input,
            TemplateDelegate template,
            TemplateDelegate ifEmpty
        );

        public static class Helpers
        {
            public static int GetIndexOffset(BindingContext context, in Arguments arguments)
            {
                if (context.Configuration.Compatibility.SupportIndexOffset 
                    && arguments.Hash.TryGetValue("indexOffset", out var indexOffsetObj) && indexOffsetObj is int offset)
                {
                    return offset;
                }

                return 0;
            }
        }
    }
}