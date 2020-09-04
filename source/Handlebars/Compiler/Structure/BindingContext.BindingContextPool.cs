using System;
using System.IO;

namespace HandlebarsDotNet
{
    public partial class BindingContext
    {
        private static readonly BindingContextPool Pool = new BindingContextPool();
        
        internal static BindingContext Create(ICompiledHandlebarsConfiguration configuration, object value, EncodedTextWriter writer, BindingContext parent)
        {
            return Pool.CreateContext(configuration, value, writer, parent, null);
        }
        
        internal static BindingContext Create(BindingContext parent, object value)
        {
            return Create(parent, value, null);
        }
        
        internal static BindingContext Create(BindingContext parent, object value, Action<BindingContext, TextWriter, object> partialBlockTemplate)
        {
            return Pool.CreateContext(parent.Configuration, value, parent.TextWriter, parent, partialBlockTemplate);
        }
        
        private class BindingContextPool : InternalObjectPool<BindingContext>
        {
            public BindingContextPool() : base(new BindingContextPolicy())
            {
            }

            public BindingContext CreateContext(
                ICompiledHandlebarsConfiguration configuration, 
                object value, 
                EncodedTextWriter writer, 
                BindingContext parent,
                Action<BindingContext, TextWriter, object> partialBlockTemplate)
            {
                var context = Get();
                context.Configuration = configuration;
                context.Value = value;
                context.TextWriter = writer;
                context.ParentContext = parent;
                context.PartialBlockTemplate = partialBlockTemplate;

                context.Initialize();

                return context;
            }
        
            private class BindingContextPolicy : IInternalObjectPoolPolicy<BindingContext>
            {
                public BindingContext Create() => new BindingContext();

                public bool Return(BindingContext item)
                {
                    item.DataObject.Clear();
                    item.BlockParams.Clear();
                    item.InlinePartialTemplates.Clear();

                    return true;
                }
            }
        }
    }
}