using System.Linq.Expressions;
using Expressions.Shortcuts;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.PathStructure;
using HandlebarsDotNet.Polyfills;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Compiler
{
    internal class IteratorBinder : HandlebarsExpressionVisitor
    {
        private CompilationContext CompilationContext { get; }
        
        public IteratorBinder(CompilationContext compilationContext)
        {
            CompilationContext = compilationContext;
        }
        
        protected override Expression VisitIteratorExpression(IteratorExpression iex)
        {
            var context = CompilationContext.Args.BindingContext;
            var writer = CompilationContext.Args.EncodedWriter;

            var template = FunctionBuilder.Compile(new[] {iex.Template}, CompilationContext);
            var ifEmpty = FunctionBuilder.Compile(new[] {iex.IfEmpty}, CompilationContext);

            if (iex.Sequence is PathExpression pathExpression)
            {
                pathExpression.Context = PathExpression.ResolutionContext.Parameter;
            }
            
            var compiledSequence = Arg<object>(FunctionBuilder.Reduce(iex.Sequence, CompilationContext));
            var blockParamsValues = CreateBlockParams();
            var arguments = FunctionBinderHelpers.CreateArguments(iex.Arguments, CompilationContext);
            
            return iex.HelperName[0] switch
            {
                '#' => Call(() => Iterator.Iterate(context, writer, blockParamsValues, arguments, compiledSequence, template, ifEmpty)),
                '^' => Call(() => Iterator.Iterate(context, writer, blockParamsValues, arguments, compiledSequence, ifEmpty, template)),
                _ => throw new HandlebarsCompilerException($"Tried to convert {iex.HelperName} expression to iterator block", iex.Context) 
            };

            ExpressionContainer<ChainSegment[]> CreateBlockParams()
            {
                var parameters = iex.BlockParams?.BlockParam?.Parameters;
                if (parameters == null)
                {
                    parameters = ArrayEx.Empty<ChainSegment>();
                }

                return Arg(parameters);
            }
        }
    }

    internal static class Iterator
    {
        public static void Iterate(
            BindingContext context,
            in EncodedTextWriter writer,
            ChainSegment[] blockParamsVariables,
            in Arguments arguments,
            object target,
            TemplateDelegate template,
            TemplateDelegate ifEmpty)
        {
            if (!HandlebarsUtils.IsTruthy(target))
            {
                using var frame = context.CreateFrame(context.Value);
                ifEmpty(writer, frame);
                return;
            }

            if (!ObjectDescriptor.TryCreate(target, out var descriptor))
            {
                throw new HandlebarsRuntimeException($"Cannot create ObjectDescriptor for type {descriptor.DescribedType}");
            }

            if (descriptor.Iterator == null) throw new HandlebarsRuntimeException($"Type {descriptor.DescribedType} does not support iteration");
            
            descriptor.Iterator.Iterate(writer, context, blockParamsVariables, arguments, target, template, ifEmpty);
        }
    }
}

