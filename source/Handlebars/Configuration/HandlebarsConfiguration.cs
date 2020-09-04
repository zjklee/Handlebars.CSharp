using HandlebarsDotNet.Compiler.Resolvers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.Helpers.BlockHelpers;

namespace HandlebarsDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HandlebarsConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, HelperDescriptorBase> Helpers { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, BlockHelperDescriptorBase> BlockHelpers { get; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, Action<TextWriter, object>> RegisteredTemplates { get; }
        
        /// <inheritdoc cref="HandlebarsDotNet.Helpers.IHelperResolver"/>
        public IList<IHelperResolver> HelperResolvers { get; }

        /// <summary>
        /// 
        /// </summary>
        public IExpressionNameResolver ExpressionNameResolver { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ITextEncoder TextEncoder { get; set; } = new HtmlEncoder();
        
        /// <inheritdoc cref="IFormatProvider"/>
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// 
        /// </summary>
        public ViewEngineFileSystem FileSystem { get; set; }

        /// <summary>
        /// 
        /// </summary>
	    public string UnresolvedBindingFormatter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ThrowOnUnresolvedBindingExpression { get; set; }

        /// <summary>
        /// The resolver used for unregistered partials. Defaults
        /// to the <see cref="FileSystemPartialTemplateResolver"/>.
        /// </summary>
        public IPartialTemplateResolver PartialTemplateResolver { get; set; } = new FileSystemPartialTemplateResolver();

        /// <summary>
        /// The handler called when a partial template cannot be found.
        /// </summary>
        public IMissingPartialTemplateHandler MissingPartialTemplateHandler { get; set; }
        
        /// <inheritdoc cref="IMemberAliasProvider"/>
        public IList<IMemberAliasProvider> AliasProviders { get; } = new ObservableList<IMemberAliasProvider>();

        /// <inheritdoc cref="HandlebarsDotNet.Compatibility"/>
        public Compatibility Compatibility { get; } = new Compatibility();

        /// <inheritdoc cref="HandlebarsDotNet.CompileTimeConfiguration"/>
        public CompileTimeConfiguration CompileTimeConfiguration { get; } = new CompileTimeConfiguration();
        
        /// <summary>
        /// 
        /// </summary>
        public HandlebarsConfiguration()
        {
            Helpers = new ObservableDictionary<string, HelperDescriptorBase>(comparer: StringComparer.OrdinalIgnoreCase);
            BlockHelpers = new ObservableDictionary<string, BlockHelperDescriptorBase>(comparer: StringComparer.OrdinalIgnoreCase);
            RegisteredTemplates = new ObservableDictionary<string, Action<TextWriter, object>>(comparer: StringComparer.OrdinalIgnoreCase);
            HelperResolvers = new ObservableList<IHelperResolver>();
        }
    }
}

