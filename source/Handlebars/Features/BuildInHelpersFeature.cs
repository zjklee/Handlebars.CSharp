using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.Helpers.BlockHelpers;

namespace HandlebarsDotNet.Features
{
    internal class BuildInHelpersFeatureFactory : IFeatureFactory
    {
        public IFeature CreateFeature()
        {
            return new BuildInHelpersFeature();
        }
    }

    [FeatureOrder(int.MinValue)]
    internal class BuildInHelpersFeature : IFeature
    {
        public void OnCompiling(ICompiledHandlebarsConfiguration configuration)
        {
            var pathInfoStore = configuration.PathInfoStore;
            configuration.BlockHelpers[pathInfoStore.GetOrAdd("with")] = new WithBlockHelperDescriptor().AsRef<BlockHelperDescriptorBase>();
            configuration.BlockHelpers[pathInfoStore.GetOrAdd("*inline")] = new InlineBlockHelperDescriptor().AsRef<BlockHelperDescriptorBase>();
            configuration.Helpers[pathInfoStore.GetOrAdd("lookup")] = new LookupReturnHelperDescriptor(configuration).AsRef<HelperDescriptorBase>();
        }

        public void CompilationCompleted()
        {
            // noting to do here
        }
    }
}