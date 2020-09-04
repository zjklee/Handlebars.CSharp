using System.IO;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet;

namespace HandlebarsDotNet.Helpers.BlockHelpers
{
    public abstract class BlockHelperDescriptorBase : IHelperDescriptor
    {
        protected BlockHelperDescriptorBase(string name) : this(TemplateContext.Shared.PathInfoStore.GetOrAdd(name))
        {
        }

        protected BlockHelperDescriptorBase(PathInfo name) => Name = name;

        public PathInfo Name { get; }
        
        public abstract HelperType Type { get; }
        
        public abstract void Invoke(TextWriter output, HelperOptions options, object context, params object[] arguments);
    }
}