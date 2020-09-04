using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Helpers
{
    public abstract class HelperDescriptor : HelperDescriptorBase
    {
        protected HelperDescriptor(string name) : base(name)
        {
        }
        
        protected HelperDescriptor(PathInfo name) : base(name)
        {
        }

        public sealed override HelperType Type { get; } = HelperType.Write;

        protected abstract void Invoke(TextWriter output, object context, params object[] arguments);

        internal sealed override object ReturnInvoke(BindingContext bindingContext, TextWriter textWriter, object[] arguments)
        {
            using var writer = ReusableStringWriter.Get(bindingContext.Configuration.FormatProvider);
            WriteInvoke(bindingContext, writer, arguments);
            return writer.ToString();
        }
        
        internal sealed override void WriteInvoke(BindingContext bindingContext, TextWriter textWriter, object[] arguments)
        {
            Invoke(textWriter, bindingContext.Value, arguments);
        }
    }
}