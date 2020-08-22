using System.IO;

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
            
            if (HandlebarsUtils.IsTruthyOrNonEmpty(arguments[0]))
            {
                using var frame = options.CreateFrame(arguments[0]);
                frame.BlockParams[0] = arguments[0];
                options.Template(output, frame);
            }
            else
            {
                options.Inverse(output, context);
            }
        }
    }
}