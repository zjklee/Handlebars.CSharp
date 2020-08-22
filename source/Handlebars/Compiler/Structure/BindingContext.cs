using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Compiler
{
    internal sealed class BindingContext : IFrame
    {
        private static readonly BindingContextPool Pool = new BindingContextPool();

        public static BindingContext Create(ICompiledHandlebarsConfiguration configuration, object value,
            EncodedTextWriter writer, BindingContext parent, string templatePath)
        {
            return Pool.CreateContext(configuration, value, writer, parent, templatePath, null);
        } 
        
        public static BindingContext Create(ICompiledHandlebarsConfiguration configuration, object value,
            EncodedTextWriter writer, BindingContext parent, string templatePath,
            Action<BindingContext, TextWriter, object> partialBlockTemplate)
        {
            return Pool.CreateContext(configuration, value, writer, parent, templatePath, partialBlockTemplate);
        }

        private BindingContext()
        {
            InlinePartialTemplates = new Dictionary<string, Action<TextWriter, object>>();
            
            ContextDataObject = new Dictionary<ChainSegment, Ref>(10);
            BlockParamsObject = new Dictionary<ChainSegment, Ref>(3);
            
            Data = new DataValues(ContextDataObject);
            BlockParams = BlockParamsValues.Empty;
        }
        
        public Dictionary<ChainSegment, Ref> ContextDataObject { get; }
        public Dictionary<ChainSegment, Ref> BlockParamsObject { get; }

        private void Initialize()
        {
            Root = ParentContext?.Root ?? this;
            
            ContextDataObject[ChainSegment.Root] = Root.Value.AsRef();
            ContextDataObject[ChainSegment.Parent] = new Ref<object>(ParentContext?.Value ?? ChainSegment.Parent.GetUndefinedBindingResult(Configuration));
            
            if (ParentContext == null) return;

            TemplatePath = ParentContext.TemplatePath ?? TemplatePath;
            
            //Inline partials cannot use the Handlebars.RegisteredTemplate method
            //because it pollutes the static dictionary and creates collisions
            //where the same partial name might exist in multiple templates.
            //To avoid collisions, pass around a dictionary of compiled partials
            //in the context
            ParentContext.InlinePartialTemplates.CopyTo(InlinePartialTemplates);

            if (!(Value is HashParameterDictionary dictionary)) return;
            
            // Populate value with parent context
            foreach (var item in GetContextDictionary(ParentContext.Value))
            {
                if (dictionary.ContainsKey(item.Key)) continue;
                dictionary[item.Key] = item.Value;
            }
        }

        public string TemplatePath { get; private set; }

        public ICompiledHandlebarsConfiguration Configuration { get; private set; }
        
        public EncodedTextWriter TextWriter { get; private set; }

        public Dictionary<string, Action<TextWriter, object>> InlinePartialTemplates { get; }

        public Action<BindingContext, TextWriter, object> PartialBlockTemplate { get; private set; }
        
        public bool SuppressEncoding
        {
            get => TextWriter.SuppressEncoding;
            set => TextWriter.SuppressEncoding = value;
        }

        public DataValues Data { get; }
        public object Value { get; set; }

        public BindingContext ParentContext { get; private set; }

        public BindingContext Root { get; private set; }

        public BlockParamsValues BlockParams { get; set; }

        public bool TryGetVariable(ChainSegment segment, out object value)
        {
            if (BlockParamsObject.TryGetValue(segment, out var valueRef))
            {
                value = valueRef.GetValue();
                return true;
            }

            value = null;
            if (Value == null) return false;

            var instanceType = Value.GetType();
            var descriptorProvider = Configuration.ObjectDescriptorProvider;
            if(
                descriptorProvider.TryGetDescriptor(instanceType, out var descriptor) &&
                descriptor.MemberAccessor.TryGetValue(Value, instanceType, segment, out value)
            )
            {
                return true;
            }

            return false;
        }
        
        public bool TryGetContextVariable(ChainSegment segment, out object value)
        {
            var hasValue = ContextDataObject.TryGetValue(segment, out var valueRef) 
                           || BlockParamsObject.TryGetValue(segment, out valueRef);

            if (hasValue)
            {
                value = valueRef.GetValue();
                return true;
            }

            value = null;
            return false;
        }

        private static IEnumerable<KeyValuePair<string, object>> GetContextDictionary(object target)
        {
            switch (target)
            {
                case null:
                    yield break;
                
                case IDictionary<string, object> dictionary:
                {
                    foreach (var item in dictionary)
                    {
                        yield return item;
                    }

                    break;
                }
                default:
                {
                    var type = target.GetType();

                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields) 
                    {
                        yield return new KeyValuePair<string, object>(field.Name, field.GetValue(target));
                    }

                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var property in properties) 
                    {
                        if (property.GetIndexParameters().Length == 0)
                        {
                            yield return new KeyValuePair<string, object>(property.Name, property.GetValue(target));
                        }
                    }

                    break;
                }
            }
        }

        public BindingContext CreateChildContext(object value, Action<BindingContext, TextWriter, object> partialBlockTemplate = null)
        {
            return Create(Configuration, value ?? Value, TextWriter, this, TemplatePath, partialBlockTemplate ?? PartialBlockTemplate);
        }
        
        public BindingContext CreateChildContext()
        {
            return Create(Configuration, null, TextWriter, this, TemplatePath, PartialBlockTemplate);
        }
        
        public void Dispose()
        {
            Pool.Return(this);
        }
        
        private class BindingContextPool : InternalObjectPool<BindingContext>
        {
            public BindingContextPool() : base(new BindingContextPolicy())
            {
            }
            
            public BindingContext CreateContext(ICompiledHandlebarsConfiguration configuration, object value, EncodedTextWriter writer, BindingContext parent, string templatePath, Action<BindingContext, TextWriter, object> partialBlockTemplate)
            {
                var context = Get();
                context.Configuration = configuration;
                context.Value = value;
                context.TextWriter = writer;
                context.ParentContext = parent;
                context.TemplatePath = templatePath;
                context.PartialBlockTemplate = partialBlockTemplate;

                context.Initialize();

                return context;
            }
        
            private class BindingContextPolicy : IInternalObjectPoolPolicy<BindingContext>
            {
                public BindingContext Create()
                {
                    return new BindingContext();
                }

                public bool Return(BindingContext item)
                {
                    item.Root = null;
                    item.Value = null;
                    item.ParentContext = null;
                    item.TemplatePath = null;
                    item.TextWriter = null;
                    item.PartialBlockTemplate = null;
                    item.BlockParams = BlockParamsValues.Empty;
                    item.InlinePartialTemplates.Clear();

                    item.BlockParamsObject.Clear();
                    item.ContextDataObject.Clear();

                    return true;
                }
            }
        }
    }
}
