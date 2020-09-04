using System.Collections.Generic;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Features;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet
{
    /// <summary>
    /// Contains compile-time affective configuration. Changing values after template compilation would take no affect.
    /// </summary>
    public class CompileTimeConfiguration
    {
        /// <inheritdoc cref="ObjectDescriptor"/>
        public IList<IObjectDescriptorProvider> ObjectDescriptorProviders { get; } = new List<IObjectDescriptorProvider>();
        
        /// <summary>
        /// 
        /// </summary>
        public IList<IExpressionMiddleware> ExpressionMiddleware { get; } = new ObservableList<IExpressionMiddleware>();
        
        /// <inheritdoc cref="IFeature"/>
        public IList<IFeatureFactory> Features { get; } = new List<IFeatureFactory>
        {
            new BuildInHelpersFeatureFactory(),
            new ClosureFeatureFactory(),
            new DefaultCompilerFeatureFactory(),
            new MissingHelperFeatureFactory()
        };

        /// <summary>
        /// The compiler used to compile <see cref="System.Linq.Expressions.Expression"/> 
        /// </summary>
        public IList<IExpressionCompiler> ExpressionCompilers { get; set; } = new ObservableList<IExpressionCompiler>();
    }
}