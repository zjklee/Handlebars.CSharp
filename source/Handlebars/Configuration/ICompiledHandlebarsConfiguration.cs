using System;
using System.Collections.Generic;
using System.IO;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Resolvers;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.Features;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.Helpers.BlockHelpers;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHandlebarsDecoratorConfiguration : IHandlebarsTemplateRegistrations
    {
        /// <summary>
        /// 
        /// </summary>
        ITextEncoder TextEncoder { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        IFormatProvider FormatProvider { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        string UnresolvedBindingFormatter { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        bool ThrowOnUnresolvedBindingExpression { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        IPartialTemplateResolver PartialTemplateResolver { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        IMissingPartialTemplateHandler MissingPartialTemplateHandler { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        IDictionary<PathInfo, Ref<HelperDescriptorBase>> Helpers { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IDictionary<PathInfo, Ref<BlockHelperDescriptorBase>> BlockHelpers { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IList<IHelperResolver> HelperResolvers { get; }
        
        /// <inheritdoc cref="IPathInfoStore"/>
        IPathInfoStore PathInfoStore { get; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public interface IHandlebarsTemplateRegistrations
    {
        /// <summary>
        /// 
        /// </summary>
        IDictionary<string, Action<TextWriter, object>> RegisteredTemplates { get; }
        ViewEngineFileSystem FileSystem { get; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public interface ICompiledHandlebarsConfiguration : IHandlebarsTemplateRegistrations
    {
        /// <summary>
        /// 
        /// </summary>
        HandlebarsConfiguration UnderlingConfiguration { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IExpressionNameResolver ExpressionNameResolver { get; }
        
        /// <summary>
        /// 
        /// </summary>
        ITextEncoder TextEncoder { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IFormatProvider FormatProvider { get; }
        
        /// <summary>
        /// 
        /// </summary>
        string UnresolvedBindingFormatter { get; }
        
        /// <summary>
        /// 
        /// </summary>
        bool ThrowOnUnresolvedBindingExpression { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IPartialTemplateResolver PartialTemplateResolver { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IMissingPartialTemplateHandler MissingPartialTemplateHandler { get; }

        /// <summary>
        /// 
        /// </summary>
        IDictionary<PathInfo, Ref<HelperDescriptorBase>> Helpers { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IDictionary<PathInfo, Ref<BlockHelperDescriptorBase>> BlockHelpers { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IList<IHelperResolver> HelperResolvers { get; }
        
        /// <inheritdoc cref="Compatibility"/>
        Compatibility Compatibility { get; }
        
        /// <inheritdoc cref="IObjectDescriptorProvider"/>
        IObjectDescriptorProvider ObjectDescriptorProvider { get; }
        
        /// <inheritdoc cref="IExpressionMiddleware"/>
        IList<IExpressionMiddleware> ExpressionMiddleware { get; }
        
        /// <inheritdoc cref="IMemberAliasProvider"/>
        IList<IMemberAliasProvider> AliasProviders { get; }
        
        /// <inheritdoc cref="IExpressionCompiler"/>
        IExpressionCompiler ExpressionCompiler { get; set; }

        /// <summary>
        /// List of associated <see cref="IFeature"/>s
        /// </summary>
        IReadOnlyList<IFeature> Features { get; }
        
        /// <inheritdoc cref="IPathInfoStore"/>
        IPathInfoStore PathInfoStore { get; }
    }
}