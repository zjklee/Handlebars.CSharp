using HandlebarsDotNet.MemberAliasProvider;

namespace HandlebarsDotNet.Features
{
    public static class CountAliasFeature
    {
        public static HandlebarsConfiguration WithCollectionMemberAlias(this  HandlebarsConfiguration configuration)
        {
            configuration.CompileTimeConfiguration.Features.Add(new Factory());

            return configuration;
        }
        
        private class Feature : IFeature
        {
            public void OnCompiling(ICompiledHandlebarsConfiguration configuration)
            {
                configuration.AliasProviders.Add(new CollectionMemberAliasProvider(configuration));
            }

            public void CompilationCompleted()
            {
                // do nothing
            }
        }
        
        private class Factory : IFeatureFactory
        {
            public IFeature CreateFeature() => new Feature();
        }
    }
}