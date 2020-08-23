using System.Linq;
using HandlebarsDotNet.Features;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet.Extension.NewtonsoftJson
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonFeatureExtensions
    {
        /// <summary>
        /// Adds <see cref="IObjectDescriptorProvider"/>s required to properly support <c>Newtonsoft.Json</c>. 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static HandlebarsConfiguration UseNewtonsoftJson(this HandlebarsConfiguration configuration)
        {
            configuration.CompileTimeConfiguration.Features.Add(new JsonFeatureFactory());
            
            return configuration;
        }
    }
    
    internal class JsonFeature : IFeature
    {
        public void OnCompiling(ICompiledHandlebarsConfiguration configuration)
        {
            var providers = configuration.ObjectDescriptorProviders;

            var objectDescriptorProvider = providers.OfType<ObjectDescriptorProvider>().Single();
            providers.Insert(0, new JArrayDescriptorProvider(objectDescriptorProvider));
            providers.Insert(0, new JObjectDescriptorProvider());
        }

        public void CompilationCompleted()
        {
            // do nothing
        }
    }
    
    internal class JsonFeatureFactory : IFeatureFactory
    {
        public IFeature CreateFeature() => new JsonFeature();
    }
}