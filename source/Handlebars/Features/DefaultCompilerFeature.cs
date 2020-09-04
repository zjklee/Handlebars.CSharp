using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Expressions.Shortcuts;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Features
{
    internal class DefaultCompilerFeatureFactory : IFeatureFactory
    {
        public IFeature CreateFeature()
        {
            return new DefaultCompilerFeature();
        }
    }

    [FeatureOrder(1)]
    internal class DefaultCompilerFeature : IFeature
    {
        public void OnCompiling(ICompiledHandlebarsConfiguration configuration)
        {
            var templateFeature = configuration.Features
                .OfType<ClosureFeature>()
                .SingleOrDefault();
            
            configuration.ExpressionMiddleware.Add(new OptimizerMiddleware());
            configuration.ExpressionCompilers.Add(new DefaultExpressionCompiler(configuration, templateFeature));
            configuration.Compiler = new DefaultCompiler(configuration);
        }

        public void CompilationCompleted()
        {
            // noting to do here
        }

        private class OptimizerMiddleware : IExpressionMiddleware
        {
            public Expression Invoke(Expression expression) => expression;
        }
        
        private class DefaultCompiler : ICompiler
        {
            private readonly ICompiledHandlebarsConfiguration _configuration;

            public DefaultCompiler(ICompiledHandlebarsConfiguration configuration)
            {
                _configuration = configuration;
            }
            
            public T Compile<T>(Expression<T> expression, params CompilerFeatures[] requiredFeatures) where T : Delegate
            {
                var expressionCompilers = _configuration.ExpressionCompilers;
                for (var compilerIndex = expressionCompilers.Count - 1; compilerIndex >= 0; compilerIndex--)
                {
                    var passed = true;
                    var expressionCompiler = expressionCompilers[compilerIndex];
                    for (var requirementIndex = 0; requirementIndex < requiredFeatures.Length; requirementIndex++)
                    {
                        var requirement = requiredFeatures[requirementIndex];
                        passed = (expressionCompiler.CompilerFeatures & requirement) == requirement;
                        if(!passed) break;
                    }
                    
                    if (passed)
                    {
                        return expressionCompiler.Compile(expression);
                    }
                }
                
                throw new HandlebarsCompilerException($"Cannot find suitable {nameof(IExpressionCompiler)} to compile expression");
            }
        }
        
        private class DefaultExpressionCompiler : IExpressionCompiler
        {
            private readonly ClosureFeature _closureFeature;
            private readonly ICollection<IExpressionMiddleware> _expressionMiddleware;

            public DefaultExpressionCompiler(ICompiledHandlebarsConfiguration configuration, ClosureFeature closureFeature)
            {
                _closureFeature = closureFeature;
                _expressionMiddleware = configuration.ExpressionMiddleware;
            }

            public CompilerFeatures CompilerFeatures { get; } = CompilerFeatures.AnonymousTypes;

            public T Compile<T>(Expression<T> expression) where T: Delegate
            {
                expression = (Expression<T>) _expressionMiddleware.Aggregate((Expression) expression, (e, m) => m.Invoke(e));
                
                var closureFeature = _closureFeature;
                var createClosureHere = false;
                
                if (closureFeature.TemplateClosure.CurrentIndex == -1)
                {
                    createClosureHere = true;
                    closureFeature = new ClosureFeature();
                    _closureFeature.Children.AddLast(closureFeature);
                }
                
                var templateClosure = closureFeature.TemplateClosure;
                var closure = closureFeature.ClosureInternal;

                expression = (Expression<T>) closureFeature.ExpressionMiddleware.Invoke(expression);
                
                if (closureFeature.TemplateClosure.CurrentIndex == 0)
                {
                    var compiledLambda = expression.Compile();
                    return compiledLambda;
                }
                else
                {
                    var parameters = new[] { (ParameterExpression) closure }.Concat(expression.Parameters);
                    var lambda = Expression.Lambda(expression.Body, parameters);
                    var compiledLambda = lambda.Compile();
                
                    var outerParameters = expression.Parameters.Select(o => Expression.Parameter(o.Type, o.Name)).ToArray();
                    var store = Arg(templateClosure).Member(o => o.Store);
                    var parameterExpressions = new[] { store.Expression }.Concat(outerParameters);
                    var invocationExpression = Expression.Invoke(Expression.Constant(compiledLambda), parameterExpressions);
                    var outerLambda = Expression.Lambda<T>(invocationExpression, outerParameters);
                    
                    if (createClosureHere)
                    {
                        closureFeature.CompilationCompleted();
                    }

                    return outerLambda.Compile();
                }
            }
        }
    }
}