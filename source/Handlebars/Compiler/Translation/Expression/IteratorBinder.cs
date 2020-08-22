using System;
using System.Linq.Expressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Expressions.Shortcuts;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;
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
            var context = Arg<BindingContext>(CompilationContext.BindingContext);

            var template = FunctionBuilder.CompileCore(new[] {iex.Template}, CompilationContext.Configuration);
            var ifEmpty = FunctionBuilder.CompileCore(new[] {iex.IfEmpty}, CompilationContext.Configuration);

            if (iex.Sequence is PathExpression pathExpression)
            {
                pathExpression.Context = PathExpression.ResolutionContext.Parameter;
            }
            
            var compiledSequence = Arg<object>(FunctionBuilder.Reduce(iex.Sequence, CompilationContext));
            var blockParamsValues = CreateBlockParams();

            return Call(() =>
                Iterator.Iterate(context, blockParamsValues, compiledSequence, template, ifEmpty)
            );
            
            ExpressionContainer<BlockParamsValues> CreateBlockParams()
            {
                var parameters = iex.BlockParams?.BlockParam?.Parameters;
                if (parameters == null)
                {
                    return Arg(BlockParamsValues.Empty);
                }

                var parametersArg = Arg(parameters);
                return Call(() => BlockParamsValues.Create(parametersArg));
            }
        }
    }

    internal static class Iterator
    {
        public static void Iterate(BindingContext context,
            BlockParamsValues blockParamsValues,
            object target,
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            if (!HandlebarsUtils.IsTruthy(target))
            {
                ifEmpty(context, context.TextWriter, context.Value);
                return;
            }

            var targetType = target.GetType();
            if (!context.Configuration.ObjectDescriptorProvider.TryGetDescriptor(targetType, out var descriptor))
            {
                ifEmpty(context, context.TextWriter, context.Value);
                return;
            }

            if (!descriptor.ShouldEnumerate)
            {
                var properties = descriptor.GetProperties(descriptor, target);
                if (properties is IList propertiesList)
                {
                    IterateObjectWithStaticProperties(context, blockParamsValues, descriptor, target, propertiesList, targetType, template, ifEmpty);
                    return;   
                }
                
                IterateObject(context, descriptor, blockParamsValues, target, properties, targetType, template, ifEmpty);
                return;
            }

            if (target is IList list)
            {
                IterateList(context, blockParamsValues, list, template, ifEmpty);
                return;
            }

            IterateEnumerable(context, blockParamsValues, (IEnumerable) target, template, ifEmpty);
        }
        
        private static void IterateObject(BindingContext context,
            ObjectDescriptor descriptor,
            BlockParamsValues blockParamsValues,
            object target,
            IEnumerable<object> properties,
            Type targetType,
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var iterator = ObjectEnumeratorValueProvider.Create(context.Configuration);
            using var innerContext = context.CreateChildContext();
            
            var accessor = descriptor.MemberAccessor;
            var enumerable = new ExtendedEnumerable<object>(properties);
            bool enumerated = false;
            
            iterator.Attach(innerContext);
            (blockParamsValues as IValueProvider).Attach(innerContext);
                    
            var iteratorValue = new Ref<object>(null);
            innerContext.ContextDataObject[ChainSegment.Value] = iteratorValue;
            blockParamsValues[0] = iteratorValue;
            blockParamsValues[1] = iterator.Key;
                    
            foreach (var enumerableValue in enumerable)
            {
                enumerated = true;
                var property = enumerableValue.Value;
                var iteratorKey = ChainSegment.Create(property as string ?? property.ToString());
                iterator.Key.Value = iteratorKey;
                iterator.First.Value = enumerableValue.IsFirst;
                iterator.Last.Value = enumerableValue.IsLast;
                iterator.Index.Value = enumerableValue.Index;

                var resolvedValue = accessor.TryGetValue(target, targetType, iteratorKey, out var value) ? value : null;
                iteratorValue.Value = resolvedValue;
                innerContext.Value = resolvedValue;
                        
                template(context, context.TextWriter, innerContext);
            }

            if (iterator.Index == 0 && !enumerated)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }

        private static void IterateObjectWithStaticProperties(BindingContext context,
            BlockParamsValues blockParamsValues,
            ObjectDescriptor descriptor,
            object target,
            IList properties,
            Type targetType,
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var iterator = ObjectEnumeratorValueProvider.Create(context.Configuration);
            using var innerContext = context.CreateChildContext();
            
            var accessor = descriptor.MemberAccessor;
            var count = properties.Count;
            iterator.Attach(innerContext);
            (blockParamsValues as IValueProvider).Attach(innerContext);
                    
            Ref iteratorValue = new Ref<object>(null);
            innerContext.ContextDataObject[ChainSegment.Value] = iteratorValue;
            blockParamsValues[0] = iteratorValue;
            blockParamsValues[1] = iterator.Key;

            var index = 0;
            for (; index < count; index++)
            {
                var property = properties[index];
                var iteratorKey = ChainSegment.Create(property as string ?? property.ToString());

                iterator.Index.Value = index;
                iterator.Key.Value = iteratorKey;
                iterator.First.Value = index == 0;
                iterator.Last.Value = index == count - 1;
                        
                var resolvedValue = accessor.TryGetValue(target, targetType, iteratorKey, out var value) ? value : null;
                iteratorValue.SetValue(resolvedValue);
                innerContext.Value = resolvedValue;

                template(context, context.TextWriter, innerContext);
            }

            if (index == 0)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
        
        private static void IterateList(BindingContext context,
            BlockParamsValues blockParamsValues,
            IList target,
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var iterator = IteratorValueProvider.Create();
            using var innerContext = context.CreateChildContext();
            
            var count = target.Count;
            iterator.Attach(innerContext);
            (blockParamsValues as IValueProvider).Attach(innerContext);

            var iteratorValue = new Ref<object>(null);
            innerContext.ContextDataObject[ChainSegment.Value] = iteratorValue;
            blockParamsValues[0] = iteratorValue;
            blockParamsValues[1] = iterator.Index;

            var index = 0;
            for (; index < count; index++)
            {
                var value = target[index];
                innerContext.Value = value;
                        
                iteratorValue.SetValue(value);
                iterator.Index.Value = index;
                iterator.First.Value = index == 0;
                iterator.Last.Value = index == count - 1;
                        
                template(context, context.TextWriter, innerContext);
            }

            if (index == 0)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
        
        private static void IterateEnumerable(BindingContext context,
            BlockParamsValues blockParamsValues,
            IEnumerable target,
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            using var iterator = IteratorValueProvider.Create();
            using var innerContext = context.CreateChildContext();
            
            var enumerated = false;
            var enumerable = new ExtendedEnumerable<object>(target);

            iterator.Attach(innerContext);
            blockParamsValues.As<IValueProvider>().Attach(innerContext);
                    
            var index = 0;
            var iteratorValue = new Ref<object>(null);
            innerContext.ContextDataObject[ChainSegment.Value] = iteratorValue;
            blockParamsValues[0] = iteratorValue;
            blockParamsValues[1] = iterator.Index;

            foreach (var enumerableValue in enumerable)
            {
                enumerated = true;
                iterator.First.Value = enumerableValue.IsFirst;
                iterator.Last.Value = enumerableValue.IsLast;
                iterator.Index.Value = index++;

                var value = enumerableValue.Value;
                iteratorValue.Value = value;
                innerContext.Value = value;
                        
                template(context, context.TextWriter, innerContext);
            }

            if (!enumerated)
            {
                ifEmpty(context, context.TextWriter, context.Value);
            }
        }
    }
}

