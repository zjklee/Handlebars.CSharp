using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Helpers
{
    public abstract class ReturnHelperDescriptor : HelperDescriptorBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected ReturnHelperDescriptor(PathInfo name) : base(name)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected ReturnHelperDescriptor(string name) : base(name)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed override HelperType Type { get; } = HelperType.Return;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected abstract object Invoke(object context, params object[] arguments);

        internal override object ReturnInvoke(BindingContext bindingContext, TextWriter textWriter, object[] arguments)
        {
            return Invoke(bindingContext.Value, arguments);
        }

        internal sealed override void WriteInvoke(BindingContext bindingContext, TextWriter textWriter, object[] arguments) => 
            bindingContext.TextWriter.Write(ReturnInvoke(bindingContext, textWriter, arguments));
    }
}