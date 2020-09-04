using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;
using HandlebarsDotNet.Features;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Extension.CompileFast
{
    internal class FastExpressionCompiler : IExpressionCompiler
    {
        private readonly ClosureFeature _closureFeature;
        private readonly ICollection<IExpressionMiddleware> _expressionMiddleware;

        public FastExpressionCompiler(ICompiledHandlebarsConfiguration configuration, ClosureFeature closureFeature)
        {
            _closureFeature = closureFeature;
            _expressionMiddleware = configuration.ExpressionMiddleware;
        }

        public CompilerFeatures CompilerFeatures { get; } = CompilerFeatures.None;

        public T Compile<T>(Expression<T> expression) where T: Delegate
        {
            expression = (Expression<T>) _expressionMiddleware.Aggregate((Expression) expression, (e, m) => m.Invoke(e));
            
            var closureFeature = _closureFeature;
                
            if (closureFeature.TemplateClosure.CurrentIndex == -1)
            {
                closureFeature = new ClosureFeature();
                _closureFeature.Children.AddLast(closureFeature);
            }
                
            var templateClosure = closureFeature.TemplateClosure;
            var closure = closureFeature.Closure;
            
            expression = (Expression<T>) _closureFeature.ExpressionMiddleware.Invoke(expression);

            if (closureFeature.TemplateClosure.CurrentIndex == 0)
            {
                var compiledLambda = expression.CompileFast();
                return compiledLambda;
            }
            else
            {
                var parameters = new[] { closure }.Concat(expression.Parameters).ToArray();
                var lambda = Expression.Lambda(expression.Body, parameters);
                var compiledDelegateType = Expression.GetDelegateType(parameters.Select(o => o.Type).Concat(new[] {lambda.ReturnType}).ToArray());
            
                var method = typeof(FastExpressionCompiler)
                    .GetMethod(nameof(CompileGeneric), BindingFlags.Static | BindingFlags.NonPublic)
                    ?.MakeGenericMethod(compiledDelegateType);
            
                var compiledLambda = method?.Invoke(null, new object[] { lambda }) ?? throw new InvalidOperationException("lambda cannot be compiled");

                var outerParameters = expression.Parameters.Select(o => Expression.Parameter(o.Type, o.Name)).ToArray();
                var store = Arg(templateClosure).Member(o => o.Store);
                var parameterExpressions = new[] { store.Expression }.Concat(outerParameters);
                var invocationExpression = Expression.Invoke(Expression.Constant(compiledLambda), parameterExpressions);
                var outerLambda = Expression.Lambda<T>(invocationExpression, outerParameters);
            
                return outerLambda.CompileFast();   
            }
        }

        private static T CompileGeneric<T>(LambdaExpression expression) where T : class
        {
            return expression.CompileFast<T>();
        }
    }
}