using System.IO;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Helpers.BlockHelpers
{
    internal sealed class WithBlockHelperDescriptor : BlockHelperDescriptor
    {
        public WithBlockHelperDescriptor() : base("with")
        {
        }

        public override void Invoke(TextWriter output, HelperOptions options, object context, params object[] arguments)
        {
            if (arguments.Length != 1)
            {
                throw new HandlebarsException("{{with}} helper must have exactly one argument");
            }

            if (!HandlebarsUtils.IsTruthyOrNonEmpty(arguments[0]))
            {
                options.Inverse(output, context);
                return;
            }

            using var frame = options.CreateFrame(arguments[0]);
            var blockParamsValues = new BlockParamsValues(frame);
            blockParamsValues[options.BlockParamsVariables[0]] = arguments[0];

            options.Template(output, frame);
        }
    }
}