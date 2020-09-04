using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Helpers
{
    internal sealed class LookupReturnHelperDescriptor : ReturnHelperDescriptor
    {
        private readonly ICompiledHandlebarsConfiguration _configuration;

        public LookupReturnHelperDescriptor(ICompiledHandlebarsConfiguration configuration) : base(TemplateContext.Shared.PathInfoStore.GetOrAdd("lookup"))
        {
            _configuration = configuration;
        }

        protected override object Invoke(object context, params object[] arguments)
        {
            if (arguments.Length != 2)
            {
                throw new HandlebarsException("{{lookup}} helper must have exactly two argument");
            }
            
            var segment = ChainSegment.Create(_configuration.TemplateContext, arguments[1]);
            return !PathResolver.TryAccessMember(arguments[0], segment, _configuration, out var value) 
                ? new UndefinedBindingResult(segment, _configuration)
                : value;
        }
    }
}