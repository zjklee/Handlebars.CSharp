using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet
{
    public partial class BindingContext : IDisposable
    {
        internal readonly Dictionary<ChainSegment, object> DataObject;
        internal readonly Dictionary<ChainSegment, object> BlockParams;

        private readonly UndefinedBindingResult _undefinedParent;
        
        private BindingContext()
        {
            _undefinedParent = ChainSegment.Parent.GetUndefinedBindingResult(Configuration);
            InlinePartialTemplates = new CascadeDictionary<string, Action<TextWriter, object>>();
            
            DataObject = new Dictionary<ChainSegment, object>(ChainSegment.DefaultEqualityComparer);
            BlockParams = new Dictionary<ChainSegment, object>();//new CascadeDictionary<ChainSegment, object>(ChainSegment.DefaultEqualityComparer);
        }
        
        private void Initialize()
        {
            Root = ParentContext?.Root ?? this;
            
            DataObject[ChainSegment.Root] = Root.Value;
            
            if (ParentContext == null)
            {
                DataObject[ChainSegment.Parent] = _undefinedParent;
                return;
            }

            DataObject[ChainSegment.Parent] = ParentContext.Value;
            ParentContext.BlockParams.CopyTo(BlockParams);

            //Inline partials cannot use the Handlebars.RegisteredTemplate method
            //because it pollutes the static dictionary and creates collisions
            //where the same partial name might exist in multiple templates.
            //To avoid collisions, pass around a dictionary of compiled partials
            //in the context
            InlinePartialTemplates.Outer = ParentContext.InlinePartialTemplates;

            if (!(Value is HashParameterDictionary dictionary) || ParentContext.Value == null || ReferenceEquals(Value, ParentContext.Value)) return;
            
            // Populate value with parent context
            PopulateHash(dictionary, ParentContext.Value, Configuration);
        }
        
        internal Action<BindingContext, TextWriter, object> PartialBlockTemplate { get; set; }
        internal CascadeDictionary<string, Action<TextWriter, object>> InlinePartialTemplates { get; }

        public BindingContext Root { get; private set; }
        public BindingContext ParentContext { get; private set; }
        
        public ICompiledHandlebarsConfiguration Configuration { get; private set; }
        public EncodedTextWriter TextWriter { get; private set; }
        
        public object Value { get; set; }

        public bool SuppressEncoding
        {
            get => TextWriter.SuppressEncoding;
            set => TextWriter.SuppressEncoding = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal BindingContext CreateChildContext(object value) 
            => Create(this, value ?? Value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal BindingContext CreateChildContext(object value, Action<BindingContext, TextWriter, object> partialBlockTemplate) 
            => Create(this, value ?? Value, partialBlockTemplate);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingContext CreateFrame(object value = null) => Create(this, value);
        
        public void Dispose() => Pool.Return(this);

        public bool TryGetContextVariable(ChainSegment segment, out object value)
        {
            return DataObject.TryGetValue(segment, out value)
                   || BlockParams.TryGetValue(segment, out value);
        }
        
        public bool TryGetVariable(ChainSegment segment, out object value)
        {
            if (BlockParams.TryGetValue(segment, out value))
            {
                return true;
            }

            return PathResolver.TryAccessMember(Value, segment, Configuration, out value);
        }
        
        private static void PopulateHash(HashParameterDictionary hash, object from, ICompiledHandlebarsConfiguration configuration)
        {
            var typeDescriptor = TypeDescriptor.Create(from, configuration);
            if (!typeDescriptor.TryGetObjectDescriptor(out var descriptor))
                throw new HandlebarsRuntimeException("Cannot populate context");

            var accessor = descriptor.MemberAccessor;
            var properties = descriptor.GetProperties(descriptor, from);
            foreach (var property in properties)
            {
                var segment = ChainSegment.Create(configuration.TemplateContext, property);
                if(hash.ContainsKey(segment)) continue;
                if (!accessor.TryGetValue(from, typeDescriptor.Type, segment, out var value)) continue;
                hash[segment] = value;
            }
        }
    }
}
