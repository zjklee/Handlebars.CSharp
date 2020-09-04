using System;
using System.Linq.Expressions;
using System.IO;
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
            var paramsVariables = new BlockParamsVariables(CompilationContext.Configuration.TemplateContext, iex.BlockParams?.BlockParams);
            var blockParamsVariables = Arg(paramsVariables);

            return Call(() =>
                Iterator.Iterate(context, blockParamsVariables, compiledSequence, template, ifEmpty)
            );
        }
    }
    
    internal static class Iterator
    {
        public static void Iterate(BindingContext context,
            BlockParamsVariables blockParamsVariables,
            object target,
            Action<BindingContext, TextWriter, object> template,
            Action<BindingContext, TextWriter, object> ifEmpty)
        {
            if (!HandlebarsUtils.IsTruthy(target))
            {
                ifEmpty(context, context.TextWriter, context.Value);
                return;
            }

            var typeDescriptor = TypeDescriptor.Create(target, context.Configuration);
            if (!typeDescriptor.TryGetObjectDescriptor(out var descriptor))
            {
                ifEmpty(context, context.TextWriter, context.Value);
                return;
            }

            if (descriptor.Iterator != null)
            {
                descriptor.Iterator.Iterate(context, blockParamsVariables, target, template, ifEmpty);
                return;
            }

            throw new NotImplementedException();
            
            // var enumerator = descriptor.GetEnumerator;
            // if (enumerator != null)
            // {
            //     var enumerable = enumerator(descriptor, target);
            //     if (enumerable is IEnumerable<KeyValuePair<object, object>> valuePairs)
            //     {
            //         IterateCustomObjectEnumerator(context, blockParamsVariables, valuePairs, template, ifEmpty);
            //     }
            //     else
            //     {
            //         IterateEnumerable(context, blockParamsVariables, enumerable, template, ifEmpty);
            //     }
            //
            //     return;
            // }
            //
            // if (!descriptor.ShouldEnumerate)
            // {
            //     var properties = descriptor.GetProperties(descriptor, target);
            //     if (properties is IList propertiesList)
            //     {
            //         IterateObjectWithStaticProperties(context, blockParamsVariables, descriptor, target, propertiesList, targetType, template, ifEmpty);
            //         return;   
            //     }
            //     
            //     IterateObject(context, descriptor, blockParamsVariables, target, properties, targetType, template, ifEmpty);
            //     return;
            // }
            //
            // if (target is IList list)
            // {
            //     IterateList(context, blockParamsVariables, list, template, ifEmpty);
            //     return;
            // }
            //
            // IterateEnumerable(context, blockParamsVariables, (IEnumerable) target, template, ifEmpty);
        }
        
        // private static void IterateObject(BindingContext context,
        //     ObjectDescriptor descriptor,
        //     ChainSegment[] blockParamsVariables,
        //     object target,
        //     IEnumerable<object> properties,
        //     Type targetType,
        //     Action<BindingContext, TextWriter, object> template,
        //     Action<BindingContext, TextWriter, object> ifEmpty)
        // {
        //     using var innerContext = context.CreateChildContext();
        //     var iterator = new ObjectEnumeratorValueProvider(innerContext);
        //     var blockParamsValues = BlockParamsValues.Create(blockParamsVariables, innerContext);
        //     
        //     var accessor = descriptor.MemberAccessor;
        //     var enumerable = new ExtendedEnumerable<object>(properties);
        //     var enumerated = false;
        //
        //     blockParamsValues[0] = iterator.CurrentValue;
        //     blockParamsValues[1] = iterator.Key;
        //             
        //     foreach (var enumerableValue in enumerable)
        //     {
        //         enumerated = true;
        //         var property = enumerableValue.Value;
        //         var iteratorKey = ChainSegment.Create(property as string ?? property.ToString());
        //         iterator.Key.Value = iteratorKey;
        //         iterator.First.Value = enumerableValue.IsFirst;
        //         iterator.Last.Value = enumerableValue.IsLast;
        //         iterator.Index.Value = enumerableValue.Index;
        //
        //         var resolvedValue = accessor.TryGetValue(target, targetType, iteratorKey, out var value) ? value : null;
        //         iterator.CurrentValue.SetValue(resolvedValue);
        //         innerContext.Value = resolvedValue;
        //
        //         template(context, context.TextWriter, innerContext);
        //     }
        //
        //     if (iterator.Index == 0 && !enumerated)
        //     {
        //         ifEmpty(context, context.TextWriter, context.Value);
        //     }
        // }
        //
        // private static void IterateObjectWithStaticProperties(BindingContext context,
        //     ChainSegment[] blockParamsVariables,
        //     ObjectDescriptor descriptor,
        //     object target,
        //     IList properties,
        //     Type targetType,
        //     Action<BindingContext, TextWriter, object> template,
        //     Action<BindingContext, TextWriter, object> ifEmpty)
        // {
        //     using var innerContext = context.CreateChildContext();
        //     var iterator = new ObjectEnumeratorValueProvider(innerContext);
        //     var blockParamsValues = BlockParamsValues.Create(blockParamsVariables, innerContext);
        //     
        //     var accessor = descriptor.MemberAccessor;
        //     var count = properties.Count;
        //     
        //     blockParamsValues[0] = iterator.CurrentValue;
        //     blockParamsValues[1] = iterator.Key;
        //
        //     var index = 0;
        //     for (; index < count; index++)
        //     {
        //         var property = properties[index];
        //         var iteratorKey = ChainSegment.Create(property as string ?? property.ToString());
        //
        //         iterator.Index.Value = index;
        //         iterator.Key.Value = iteratorKey;
        //         iterator.First.Value = index == 0;
        //         iterator.Last.Value = index == count - 1;
        //                 
        //         var resolvedValue = accessor.TryGetValue(target, targetType, iteratorKey, out var value) ? value : null;
        //         iterator.CurrentValue.SetValue(resolvedValue);
        //         innerContext.Value = resolvedValue;
        //
        //         template(context, context.TextWriter, innerContext);
        //     }
        //
        //     if (index == 0)
        //     {
        //         ifEmpty(context, context.TextWriter, context.Value);
        //     }
        // }
        //
        // private static void IterateList(BindingContext context,
        //     ChainSegment[] blockParamsVariables,
        //     IList target,
        //     Action<BindingContext, TextWriter, object> template,
        //     Action<BindingContext, TextWriter, object> ifEmpty)
        // {
        //     using var innerContext = context.CreateChildContext();
        //     var iterator = new IteratorValueProvider(innerContext);
        //     var blockParamsValues = BlockParamsValues.Create(blockParamsVariables, innerContext);
        //
        //     var count = target.Count;
        //
        //     blockParamsValues[0] = iterator.CurrentValue;
        //     blockParamsValues[1] = iterator.Index;
        //
        //     var index = 0;
        //     for (; index < count; index++)
        //     {
        //         var value = target[index];
        //         innerContext.Value = value;
        //                 
        //         iterator.CurrentValue.SetValue(value);
        //         iterator.Index.Value = index;
        //         iterator.First.Value = index == 0;
        //         iterator.Last.Value = index == count - 1;
        //                 
        //         template(context, context.TextWriter, innerContext);
        //     }
        //
        //     if (index == 0)
        //     {
        //         ifEmpty(context, context.TextWriter, context.Value);
        //     }
        // }
        //
        // private static void IterateEnumerable(BindingContext context,
        //     ChainSegment[] blockParamsVariables,
        //     IEnumerable target,
        //     Action<BindingContext, TextWriter, object> template,
        //     Action<BindingContext, TextWriter, object> ifEmpty)
        // {
        //     using var innerContext = context.CreateChildContext();
        //     var iterator = new IteratorValueProvider(innerContext);
        //     var blockParamsValues = BlockParamsValues.Create(blockParamsVariables, innerContext);
        //
        //     var enumerated = false;
        //     var enumerable = new ExtendedEnumerable2<object>(target);
        //
        //     var index = 0;
        //     blockParamsValues[0] = iterator.CurrentValue;
        //     blockParamsValues[1] = iterator.Index;
        //
        //     foreach (var enumerableValue in enumerable)
        //     {
        //         enumerated = true;
        //         iterator.First.Value = enumerableValue.IsFirst;
        //         iterator.Last.Value = enumerableValue.IsLast;
        //         iterator.Index.Value = index++;
        //
        //         iterator.CurrentValue.Value = innerContext.Value = enumerableValue.Value;
        //
        //         template(context, context.TextWriter, innerContext);
        //     }
        //
        //     if (!enumerated)
        //     {
        //         ifEmpty(context, context.TextWriter, context.Value);
        //     }
        // }
        //
        // private static void IterateCustomObjectEnumerator(BindingContext context,
        //     ChainSegment[] blockParamsVariables,
        //     IEnumerable<KeyValuePair<object, object>> properties,
        //     Action<BindingContext, TextWriter, object> template,
        //     Action<BindingContext, TextWriter, object> ifEmpty)
        // {
        //     using var innerContext = context.CreateChildContext();
        //     var iterator = new ObjectEnumeratorValueProvider(innerContext);
        //     var blockParamsValues = BlockParamsValues.Create(blockParamsVariables, innerContext);
        //
        //     var enumerable = new ExtendedEnumerable<KeyValuePair<object, object>>(properties);
        //     var enumerated = false;
        //     
        //     blockParamsValues[0] = iterator.CurrentValue;
        //     blockParamsValues[1] = iterator.Key;
        //     
        //     foreach (var enumerableValue in enumerable)
        //     {
        //         enumerated = true;
        //         var property = enumerableValue.Value;
        //         iterator.Key.Value = property.Key;
        //         iterator.First.Value = enumerableValue.IsFirst;
        //         iterator.Last.Value = enumerableValue.IsLast;
        //         iterator.Index.Value = enumerableValue.Index;
        //
        //         iterator.CurrentValue.SetValue(property.Value);
        //         innerContext.Value = property.Value;
        //         
        //         template(context, context.TextWriter, innerContext);
        //     }
        //
        //     if (iterator.Index == 0 && !enumerated)
        //     {
        //         ifEmpty(context, context.TextWriter, context.Value);
        //     }
        // }
    }
}

