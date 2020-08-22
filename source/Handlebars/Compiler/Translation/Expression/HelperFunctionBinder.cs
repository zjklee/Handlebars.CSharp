using System.Linq;
using System.Linq.Expressions;
using Expressions.Shortcuts;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Helpers;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Compiler
{
    internal class HelperFunctionBinder : HandlebarsExpressionVisitor
    {
        private CompilationContext CompilationContext { get; }

        public HelperFunctionBinder(CompilationContext compilationContext)
        {
            CompilationContext = compilationContext;
        }
        
        protected override Expression VisitStatementExpression(StatementExpression sex)
        {
            return sex.Body is HelperExpression ? Visit(sex.Body) : sex;
        }
        
        protected override Expression VisitHelperExpression(HelperExpression hex)
        {
            var pathInfo = CompilationContext.Configuration.PathInfoStore.GetOrAdd(hex.HelperName);
            if(!pathInfo.IsValidHelperLiteral && !CompilationContext.Configuration.Compatibility.RelaxedHelperNaming) return Expression.Empty();
            
            var helperName = pathInfo.TrimmedPath;
            var bindingContext = Arg<BindingContext>(CompilationContext.BindingContext);
            var contextValue = bindingContext.Property(o => o.Value);
            var textWriter = bindingContext.Property(o => o.TextWriter);
            var arguments = hex.Arguments
                .ApplyOn<Expression, PathExpression>(path => path.Context = PathExpression.ResolutionContext.Parameter)
                .Select(o => FunctionBuilder.Reduce(o, CompilationContext));
            
            var args = Array<object>(arguments);

            var configuration = CompilationContext.Configuration;
            if (configuration.Helpers.TryGetValue(pathInfo, out var helper))
            {
                return Call(() => helper.Value.WriteInvoke(bindingContext, textWriter, contextValue, args));
            }
            
            for (var index = 0; index < configuration.HelperResolvers.Count; index++)
            {
                var resolver = configuration.HelperResolvers[index];
                if (resolver.TryResolveHelper(helperName, typeof(object), out var resolvedHelper))
                {
                    return Call(() => resolvedHelper.WriteInvoke(bindingContext, textWriter, contextValue, args));
                }
            }

            var lateBindDescriptor = new LateBindHelperDescriptor(pathInfo, configuration).AsRef<HelperDescriptorBase>();
            configuration.Helpers.Add(pathInfo, lateBindDescriptor);
            
            return Call(() => lateBindDescriptor.Value.WriteInvoke(bindingContext, textWriter, contextValue, args));
        }
    }
}
