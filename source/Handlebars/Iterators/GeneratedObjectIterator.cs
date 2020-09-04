using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Expressions.Shortcuts;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.Structure.Path;
using HandlebarsDotNet.ValueProviders;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Iterators
{
    internal sealed class GeneratedObjectIterator : Iterator
    {
        private static readonly MethodInfo DelegateGeneratorMethodInfo = typeof(GeneratedObjectIterator)
            .GetMethod(nameof(DelegateGenerator), BindingFlags.Static | BindingFlags.NonPublic);
            
        private static readonly PropertyInfo BlockParamsValuesIndexer = typeof(BlockParamsValues)
            .GetProperties().Single(o => o.GetIndexParameters().Length != 0);
        
        private readonly Lazy<Action<BindingContext, ChainSegment[], object, Action<BindingContext, TextWriter, object>, Action<BindingContext, TextWriter, object>>> _generatedDelegate;

        public GeneratedObjectIterator(ICompiledHandlebarsConfiguration configuration, ObjectDescriptor descriptor) : base(descriptor)
        {
            _generatedDelegate = new Lazy<Action<BindingContext, ChainSegment[], object, Action<BindingContext, TextWriter, object>, Action<BindingContext, TextWriter, object>>>(
                () => (Action<BindingContext, ChainSegment[], object, Action<BindingContext, TextWriter, object>, Action<BindingContext, TextWriter, object>>) 
                                DelegateGeneratorMethodInfo
                                    .MakeGenericMethod(descriptor.DescribedType)
                                    .Invoke(null, new object[]{ configuration, descriptor })
            );
        }

        public override void Iterate(BindingContext context, ChainSegment[] blockParamsVariables, object target, Action<BindingContext, TextWriter, object> template, Action<BindingContext, TextWriter, object> ifEmpty)
        {
            _generatedDelegate.Value(context, blockParamsVariables, target, template, ifEmpty);
        }
        
        private static Action<BindingContext, ChainSegment[], object, Action<BindingContext, TextWriter, object>, Action<BindingContext, TextWriter, object>> DelegateGenerator<T>(ICompiledHandlebarsConfiguration configuration, ObjectDescriptor descriptor)
        {
            var properties = descriptor.GetProperties(descriptor, null) as string[];
            
            var parameters = Block()
                .Parameter<BindingContext>(out var context)
                .Parameter<ChainSegment[]>(out var blockParamsVariables)
                .Parameter<Value>(out var targetObj)
                .Parameter<Action<BindingContext, TextWriter, object>>(out var template)
                .Parameter<Action<BindingContext, TextWriter, object>>(out var ifEmpty)
                .Parameters;

            var textWriter = context.Member(o => o.TextWriter);
            var value = context.Member(o => o.Value);
            
            if (properties!.Length == 0)
            {
                var ifEmptyCall = ifEmpty.Call(o => o(context, textWriter, value));

                var lambda = Expression.Lambda<Action<BindingContext, ChainSegment[], object, Action<BindingContext, TextWriter, object>, Action<BindingContext, TextWriter, object>>>(ifEmptyCall, parameters);
                return configuration.Compiler.Compile(lambda);
            }

            var block = Block()
                .Parameter<BindingContext>(out var innerContext, context.Call(o => o.CreateFrame(null)))
                .Parameter<ObjectEnumeratorValueProvider<string, object>>(out var iterator)
                .Parameter<BlockParamsValues>(out var blockParamsValues)
                .Line(Try().Finally(innerContext.Call(o => o.Dispose())).Body(builder =>
                {
                    var currentValueRef = iterator.Member(o => o.CurrentValue);
                    var currentValue = iterator.Member(o => o.CurrentValue.Self);
                    var keyRef = iterator.Member(o => o.Key);
                    var key = iterator.Member(o => o.Key.Self);
                    var index = iterator.Member(o => o.Index.Self);
                    var first = iterator.Member(o => o.First.Self);
                    var last = iterator.Member(o => o.Last.Self);

                    builder
                        .Parameter<T>(out var target, targetObj.Cast<Value<T>>().Member(o => o.Self))
                        .Lines(new Expression[]
                        {
                            iterator.Assign(New(() => new ObjectEnumeratorValueProvider<string, object>(innerContext))),
                            blockParamsValues.Assign(New(() => new BlockParamsValues(blockParamsVariables, innerContext))),
                            MakeIndex(blockParamsValues, 0, currentValue),
                            MakeIndex(blockParamsValues, 1, key),
                            innerContext.Member(o => o.Value).Assign(iterator.Member(o => o.CurrentValue)),

                            first.Assign(Arg(true))
                        });

                    if (configuration.Compatibility.SupportLastInObjectIterations)
                    {
                        builder.Line(last.Assign(Arg(false)));
                    }
                    
                    var indexValue = 0;
                    var lastIndex = properties.Length - 1;
                    for (; indexValue < properties.Length; indexValue++)
                    {
                        var property = properties[indexValue];

                        builder.Lines(new Expression[]
                        {
                            key.Assign(Arg(property)),
                            index.Assign(Arg(indexValue)),
                            currentValue.Assign(Expression.PropertyOrField(target, property)),
                        });

                        if (indexValue == 1 && indexValue != lastIndex)
                        {
                            builder.Line(first.Assign(Arg(false)));
                        }

                        if (indexValue == lastIndex && configuration.Compatibility.SupportLastInObjectIterations)
                        {
                            builder.Line(last.Assign(Arg(true)));
                        }

                        builder.Line(
                            Expression.Invoke(template, context, textWriter, innerContext)
                        );
                    }
                    
                    builder.Lines(new Expression[]
                    {
                        currentValueRef.Call(o => o.Return()),
                        keyRef.Call(o => o.Return())
                    });
                }));

            var expression = Expression.Lambda<Action<BindingContext, ChainSegment[], object, Action<BindingContext, TextWriter, object>, Action<BindingContext, TextWriter, object>>>(
                block, parameters
            );

            return configuration.Compiler.Compile(expression, CompilerFeatures.AnonymousTypes);

            static Expression MakeIndex(ExpressionContainer<BlockParamsValues> expressionContainer, int index, Expression setValue)
            {
                var indexExpression = Expression.MakeIndex(expressionContainer, BlockParamsValuesIndexer, new Expression[]{ Arg(index) });
                
                return Expression.Assign(indexExpression, setValue);
            }
        }
    }
}