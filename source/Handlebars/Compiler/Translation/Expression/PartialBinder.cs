using System;
using System.IO;
using System.Linq.Expressions;
using Expressions.Shortcuts;
using HandlebarsDotNet.Adapters;

namespace HandlebarsDotNet.Compiler
{
    internal class PartialBinder : HandlebarsExpressionVisitor
    {
        private static string SpecialPartialBlockName = "@partial-block";

        private CompilationContext CompilationContext { get; }
        
        public PartialBinder(CompilationContext compilationContext)
        {
            CompilationContext = compilationContext;
        }
        
        protected override Expression VisitBlockHelperExpression(BlockHelperExpression bhex) => bhex;

        protected override Expression VisitStatementExpression(StatementExpression sex) => sex.Body is PartialExpression ? Visit(sex.Body) : sex;

        protected override Expression VisitPartialExpression(PartialExpression pex)
        {
            var bindingContext = ExpressionShortcuts.Arg<BindingContext>(CompilationContext.BindingContext);
            var partialBlockTemplate = pex.Fallback != null 
                ? FunctionBuilder.CompileCore(new[] { pex.Fallback }, CompilationContext.Configuration) 
                : null;

            if (pex.Argument != null || partialBlockTemplate != null)
            {
                var value = ExpressionShortcuts.Arg<object>(FunctionBuilder.Reduce(pex.Argument, CompilationContext));
                var partialTemplate = ExpressionShortcuts.Arg(partialBlockTemplate);
                bindingContext = bindingContext.Call(o => o.CreateChildContext(value, partialTemplate));
            }

            var partialName = ExpressionShortcuts.Cast<string>(pex.PartialName);
            return ExpressionShortcuts.Call(() =>
                InvokePartialWithFallback(partialName, bindingContext)
            );
        }

        private static void InvokePartialWithFallback(string partialName, BindingContext context)
        {
            var partialBlockTemplate = context.PartialBlockTemplate;
            
            if (InvokePartial(partialName, context, partialBlockTemplate)) return;
            if (partialBlockTemplate == null)
            {
                var configuration = context.Configuration;
                if (configuration.MissingPartialTemplateHandler == null)
                    throw new HandlebarsRuntimeException($"Referenced partial name {partialName} could not be resolved");
                
                configuration.MissingPartialTemplateHandler.Handle(configuration, partialName, context.TextWriter);
                return;
            }

            partialBlockTemplate(context, context.TextWriter, context);
        }

        private static bool InvokePartial(string partialName, BindingContext context, Action<BindingContext, TextWriter, object> partialBlockTemplate)
        {
            if (partialName.Equals(SpecialPartialBlockName))
            {
                if (partialBlockTemplate == null)
                {
                    return false;
                }

                partialBlockTemplate(context, context.TextWriter, context);
                return true;
            }

            //if we have an inline partial, skip the file system and RegisteredTemplates collection
            if (context.InlinePartialTemplates.TryGetValue(partialName, out var partial))
            {
                partial(context.TextWriter, context);
                return true;
            }

            var configuration = context.Configuration;
            // Partial is not found, so call the resolver and attempt to load it.
            if (configuration.RegisteredTemplates.TryGetValue(partialName, out var registeredPartial))
            {
                Invoke(partialName, context, registeredPartial);
                return true;
            }

            var templateResolver = configuration.PartialTemplateResolver;
            if (templateResolver == null || !templateResolver.TryResolvePartial(configuration, partialName, out partial))
            {
                // Template not found.
                return false;
            }
            
            configuration.RegisteredTemplates.Add(partialName, partial);
            Invoke(partialName, context, partial);
            return true;
            
            static void Invoke(string partialName, BindingContext bindingContext, Action<TextWriter, object> action)
            {
                try
                {
                    action(bindingContext.TextWriter, bindingContext);
                }
                catch (Exception exception)
                {
                    throw new HandlebarsRuntimeException(
                        $"Runtime error while rendering partial '{partialName}', see inner exception for more information",
                        exception);
                }
            }
        }
    }
}
