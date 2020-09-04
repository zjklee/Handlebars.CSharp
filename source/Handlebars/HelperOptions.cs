using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet
{
    /// <summary>
    /// Contains properties accessible withing <see cref="HandlebarsBlockHelper"/> function 
    /// </summary>
    public sealed class HelperOptions : IDisposable
    {
        private static readonly InternalObjectPool<HelperOptions> Pool = new InternalObjectPool<HelperOptions>(new Policy());
        
        private readonly Dictionary<string, object> _extensions;

        internal static HelperOptions Create(Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> inverse,
            BlockParamsVariables blockParamsVariables,
            BindingContext bindingContext)
        {
            var item = Pool.Get();

            item.OriginalTemplate = template;
            item.OriginalInverse = inverse;
            
            item.BindingContext = bindingContext;
            item.Configuration = bindingContext.Configuration;
            item.BlockParamsVariables = blockParamsVariables;

            return item;
        }
        
        private HelperOptions()
        {
            _extensions = new Dictionary<string, object>(7);
            Template = (writer, o) => OriginalTemplate(BindingContext, writer, o);
            Inverse = (writer, o) => OriginalInverse(BindingContext, writer, o);
        }

        /// <summary>
        /// BlockHelper body
        /// </summary>
        public Action<TextWriter, object> Template { get; }

        /// <summary>
        /// BlockHelper <c>else</c> body
        /// </summary>
        public Action<TextWriter, object> Inverse { get; }

        public BlockParamsVariables BlockParamsVariables { get; private set; }
        
        internal ICompiledHandlebarsConfiguration Configuration { get; private set; }
        internal BindingContext BindingContext { get; private set; }
        internal Action<BindingContext, TextWriter, object> OriginalTemplate { get; private set; }
        internal Action<BindingContext, TextWriter, object> OriginalInverse { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingContext CreateFrame(object value = null) => BindingContext.CreateChildContext(value);

        /// <summary>
        /// Provides access to dynamic data entries
        /// </summary>
        /// <param name="property"></param>
        public object this[string property]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _extensions.TryGetValue(property, out var value) ? value : null;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => _extensions[property] = value;
        }

        /// <summary>
        /// Provides access to dynamic data entries in a typed manner
        /// </summary>
        /// <param name="property"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue<T>(string property) where T : class => 
            (_extensions.TryGetValue(property, out var value) ? value : null) as T;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Pool.Return(this);

        private class Policy : IInternalObjectPoolPolicy<HelperOptions>
        {
            public HelperOptions Create() => new HelperOptions();

            public bool Return(HelperOptions item)
            {
                item._extensions.Clear();
                
                return true;
            }
        }
    }
}

