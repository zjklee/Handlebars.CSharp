using System.IO;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Helpers.BlockHelpers
{
    internal sealed class LateBindBlockHelperDescriptor : BlockHelperDescriptor
    {
        private readonly ICompiledHandlebarsConfiguration _configuration;
        private readonly Ref<BlockHelperDescriptorBase> _blockHelperMissing;

        public LateBindBlockHelperDescriptor(string name, ICompiledHandlebarsConfiguration configuration) : base(configuration.PathInfoStore.GetOrAdd(name))
        {
            _configuration = configuration;
            var pathInfoStore = _configuration.PathInfoStore;
            _blockHelperMissing = _configuration.BlockHelpers[pathInfoStore.GetOrAdd("blockHelperMissing")];
        }
        
        public LateBindBlockHelperDescriptor(PathInfo name, ICompiledHandlebarsConfiguration configuration) : base(name)
        {
            _configuration = configuration;
            var pathInfoStore = _configuration.PathInfoStore;
            _blockHelperMissing = _configuration.BlockHelpers[pathInfoStore.GetOrAdd("blockHelperMissing")];
        }
        
        public override void Invoke(TextWriter output, HelperOptions options, object context, params object[] arguments)
        {
            var helperResolvers = (ObservableList<IHelperResolver>)_configuration.HelperResolvers;
            if(helperResolvers.Count != 0)
            {
                for (var index = 0; index < helperResolvers.Count; index++)
                {
                    if (!helperResolvers[index].TryResolveBlockHelper(Name, out var descriptor)) continue;

                    descriptor.Invoke(output, options, context, arguments);
                    return;
                }
            }

            options["name"] = Name.TrimmedPath;
            options["path"] = Name;
            _blockHelperMissing.Value.Invoke(output, options, context, arguments);
        }
    }
}