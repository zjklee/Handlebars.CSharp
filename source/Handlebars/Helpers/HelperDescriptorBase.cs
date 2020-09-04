using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;

namespace HandlebarsDotNet.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class HelperDescriptorBase : IHelperDescriptor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected HelperDescriptorBase(string name) : this(TemplateContext.Shared.PathInfoStore.GetOrAdd(name))
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected HelperDescriptorBase(PathInfo name) => Name = name;

        public PathInfo Name { get; }
        public abstract HelperType Type { get; }

        internal abstract object ReturnInvoke(BindingContext bindingContext, TextWriter textWriter, object[] arguments);

        internal abstract void WriteInvoke(BindingContext bindingContext, TextWriter textWriter, object[] arguments);
        
        /// <summary>
        /// Returns helper name
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;
    }
}