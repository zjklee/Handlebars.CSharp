using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Helpers
{
    internal sealed class LookupReturnHelperDescriptor : ReturnHelperDescriptor
    {
        private readonly ICompiledHandlebarsConfiguration _configuration;

        public LookupReturnHelperDescriptor(ICompiledHandlebarsConfiguration configuration) : base(configuration.PathInfoStore.GetOrAdd("lookup"))
        {
            _configuration = configuration;
        }

        public override object Invoke(object context, params object[] arguments)
        {
            if (arguments.Length != 2)
            {
                throw new HandlebarsException("{{lookup}} helper must have exactly two argument");
            }
            
            var memberName = arguments[1].ToString();
            var segment = ChainSegment.Create(memberName);
            return !PathResolver.TryAccessMember(arguments[0], segment, _configuration, out var value) 
                ? new UndefinedBindingResult(memberName, _configuration)
                : value;
        }
    }
}