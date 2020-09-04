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
            configuration.BlockHelpers[pathInfoStore.GetOrAdd("with")] = Ref.Create<BlockHelperDescriptorBase>(new WithBlockHelperDescriptor());
            configuration.BlockHelpers[pathInfoStore.GetOrAdd("*inline")] = Ref.Create<BlockHelperDescriptorBase>(new InlineBlockHelperDescriptor());
            configuration.Helpers[pathInfoStore.GetOrAdd("lookup")] = Ref.Create<HelperDescriptorBase>(new LookupReturnHelperDescriptor(configuration));
        }

        public void CompilationCompleted()
        {
            // noting to do here
        }
    }
}