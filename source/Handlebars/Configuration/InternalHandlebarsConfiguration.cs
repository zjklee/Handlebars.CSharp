using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Resolvers;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.Features;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.Helpers.BlockHelpers;
using HandlebarsDotNet.MemberAliasProvider;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet
{
    internal class HandlebarsConfigurationAdapter : ICompiledHandlebarsConfiguration
    {
        public HandlebarsConfigurationAdapter(HandlebarsConfiguration configuration)
        {
            UnderlingConfiguration = configuration;

            TemplateContext = new TemplateContext();
            PathInfoStore = new PathInfoStore(TemplateContext);
            
            HelperResolvers = new ObservableList<IHelperResolver>(configuration.HelperResolvers);
            RegisteredTemplates = new ObservableDictionary<string, Action<TextWriter, object>>(configuration.RegisteredTemplates);
            AliasProviders = new ObservableList<IMemberAliasProvider>(UnderlingConfiguration.AliasProviders);
            ObjectDescriptorProvider = CreateObjectDescriptorProvider();

            ExpressionMiddleware = new ObservableList<IExpressionMiddleware>(UnderlingConfiguration.CompileTimeConfiguration.ExpressionMiddleware);
            ExpressionCompilers = new ObservableList<IExpressionCompiler>(UnderlingConfiguration.CompileTimeConfiguration.ExpressionCompilers);

            Features = UnderlingConfiguration.CompileTimeConfiguration.Features
                .Select(o => o.CreateFeature())
                .OrderBy(o => o.GetType().GetTypeInfo().GetCustomAttribute<FeatureOrderAttribute>()?.Order ?? 100)
                .ToList();
            
            CreateHelpersSubscription();
            CreateBlockHelpersSubscription();
            
            AliasProviders.Add(new CollectionMemberAliasProvider(this));
            TemplateProperties = new TemplateProperties();
        }

        public TemplateContext TemplateContext { get; }
        public HandlebarsConfiguration UnderlingConfiguration { get; }
        public IExpressionNameResolver ExpressionNameResolver => UnderlingConfiguration.ExpressionNameResolver;
        public ITextEncoder TextEncoder => UnderlingConfiguration.TextEncoder;
        public IFormatProvider FormatProvider => UnderlingConfiguration.FormatProvider;
        public ViewEngineFileSystem FileSystem => UnderlingConfiguration.FileSystem;
        public string UnresolvedBindingFormatter => UnderlingConfiguration.UnresolvedBindingFormatter;
        public bool ThrowOnUnresolvedBindingExpression => UnderlingConfiguration.ThrowOnUnresolvedBindingExpression;
        public IPartialTemplateResolver PartialTemplateResolver => UnderlingConfiguration.PartialTemplateResolver;
        public IMissingPartialTemplateHandler MissingPartialTemplateHandler => UnderlingConfiguration.MissingPartialTemplateHandler;
        public Compatibility Compatibility => UnderlingConfiguration.Compatibility;
        
        public IObjectDescriptorProvider ObjectDescriptorProvider { get; }
        public IList<IExpressionMiddleware> ExpressionMiddleware { get; }
        public IList<IMemberAliasProvider> AliasProviders { get; }
        public IList<IObjectDescriptorProvider> ObjectDescriptorProviders { get; private set; }
        public IList<IExpressionCompiler> ExpressionCompilers { get; set; }
        
        public ICompiler Compiler { get; set; }
        public IReadOnlyList<IFeature> Features { get; }
        
        public IPathInfoStore PathInfoStore { get; }
        
        public TemplateProperties TemplateProperties { get; set; }

        public IDictionary<PathInfo, Ref<HelperDescriptorBase>> Helpers { get; private set; }
        public IDictionary<PathInfo, Ref<BlockHelperDescriptorBase>> BlockHelpers { get; private set; }
        public IList<IHelperResolver> HelperResolvers { get; }
        public IDictionary<string, Action<TextWriter, object>> RegisteredTemplates { get; }
        
        private void CreateHelpersSubscription()
        {
            var existingHelpers = UnderlingConfiguration.Helpers.ToDictionary(
                o => PathInfoStore.GetOrAdd($"[{o.Key}]"),
                o => new Ref<HelperDescriptorBase>(o.Value)
            );

            Helpers = new ObservableDictionary<PathInfo, Ref<HelperDescriptorBase>>(existingHelpers, Compatibility.RelaxedHelperNaming ? PathInfo.PlainPathComparer : PathInfo.PlainPathWithPartsCountComparer);
            
            var helpersObserver = new ObserverBuilder<ObservableEvent<HelperDescriptorBase>>()
                .OnEvent<ObservableDictionary<string, HelperDescriptorBase>.ReplacedObservableEvent>(
                    @event => Helpers[PathInfoStore.GetOrAdd($"[{@event.Key}]")].Value = @event.Value
                    )
                .OnEvent<ObservableDictionary<string, HelperDescriptorBase>.AddedObservableEvent>(
                    @event =>
                    {
                        Helpers.AddOrUpdate(PathInfoStore.GetOrAdd($"[{@event.Key}]"), 
                            Ref.Create, 
                            (h, o) => o.Value = h, 
                            @event.Value);
                    })
                .OnEvent<ObservableDictionary<string, HelperDescriptorBase>.RemovedObservableEvent>(@event =>
                {
                    if (Helpers.TryGetValue(PathInfoStore.GetOrAdd($"[{@event.Key}]"), out var helperToRemove))
                    {
                        helperToRemove.Value = new LateBindHelperDescriptor(@event.Key, this);
                    }
                })
                .Build();

            UnderlingConfiguration.Helpers
                .As<ObservableDictionary<string, HelperDescriptorBase>>()
                .Subscribe(helpersObserver);
        }

        private void CreateBlockHelpersSubscription()
        {
            var existingBlockHelpers = UnderlingConfiguration.BlockHelpers.ToDictionary(
                o => PathInfoStore.GetOrAdd($"[{o.Key}]"),
                o => new Ref<BlockHelperDescriptorBase>(o.Value)
            );

            BlockHelpers =
                new ObservableDictionary<PathInfo, Ref<BlockHelperDescriptorBase>>(existingBlockHelpers, Compatibility.RelaxedHelperNaming ? PathInfo.PlainPathComparer : PathInfo.PlainPathWithPartsCountComparer);

            var blockHelpersObserver = new ObserverBuilder<ObservableEvent<BlockHelperDescriptorBase>>()
                .OnEvent<ObservableDictionary<string, BlockHelperDescriptorBase>.ReplacedObservableEvent>(
                    @event => BlockHelpers[PathInfoStore.GetOrAdd($"[{@event.Key}]")].Value = @event.Value)
                .OnEvent<ObservableDictionary<string, BlockHelperDescriptorBase>.AddedObservableEvent>(
                    @event =>
                    {
                        BlockHelpers.AddOrUpdate(PathInfoStore.GetOrAdd($"[{@event.Key}]"), 
                            Ref.Create, 
                            (h, o) => o.Value = h, 
                            @event.Value);
                    })
                .OnEvent<ObservableDictionary<string, BlockHelperDescriptorBase>.RemovedObservableEvent>(@event =>
                {
                    if (BlockHelpers.TryGetValue(PathInfoStore.GetOrAdd($"[{@event.Key}]"), out var helperToRemove))
                    {
                        helperToRemove.Value = new LateBindBlockHelperDescriptor(@event.Key, this);
                    }
                })
                .Build();

            UnderlingConfiguration.BlockHelpers
                .As<ObservableDictionary<string, BlockHelperDescriptorBase>>()
                .Subscribe(blockHelpersObserver);
        }

        private IObjectDescriptorProvider CreateObjectDescriptorProvider()
        {
            var objectDescriptorProvider = new ObjectDescriptorProvider(this);
            List<IObjectDescriptorProvider> providers;
            ObjectDescriptorProviders = providers = new List<IObjectDescriptorProvider>(UnderlingConfiguration.CompileTimeConfiguration.ObjectDescriptorProviders)
            {
                //new PrimitiveTypesObjectDescriptorProvider(),
                new RefObjectDescriptor(this),
                //new ContextObjectDescriptor(),
                new StringDictionaryObjectDescriptorProvider(),
                new GenericDictionaryObjectDescriptorProvider(),
                new DictionaryObjectDescriptor(),
                new KeyValuePairObjectDescriptorProvider(),
                //new CollectionObjectDescriptor(objectDescriptorProvider),
                new EnumerableObjectDescriptor(objectDescriptorProvider),
                objectDescriptorProvider,
                new DynamicObjectDescriptor(this)
            };

            return new ObjectDescriptorFactory(providers);
        }
    }
}