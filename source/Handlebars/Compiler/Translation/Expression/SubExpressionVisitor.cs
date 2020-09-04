﻿using System.IO;
using System.Linq.Expressions;
using Expressions.Shortcuts;
using HandlebarsDotNet.Helpers;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Compiler
{
    internal class SubExpressionVisitor : HandlebarsExpressionVisitor
    {
        private CompilationContext CompilationContext { get; }

        public SubExpressionVisitor(CompilationContext compilationContext)
        {
            CompilationContext = compilationContext;
        }
        
        protected override Expression VisitSubExpression(SubExpressionExpression subex)
        {
            switch (subex.Expression)
            {
                case MethodCallExpression callExpression:
                    return HandleMethodCallExpression(callExpression);
                
                default:
                    var expression = FunctionBuilder.Reduce(subex.Expression, CompilationContext);
                    if (expression is MethodCallExpression lateBoundCall)
                        return HandleMethodCallExpression(lateBoundCall);
                    
                    throw new HandlebarsCompilerException("Sub-expression does not contain a converted MethodCall expression");
            }
        }

        private static Expression HandleMethodCallExpression(MethodCallExpression helperCall)
        {
            var bindingContext = Arg<BindingContext>(helperCall.Arguments[0]);
            var textWriter = Arg<TextWriter>(helperCall.Arguments[1]);
            var arguments = Arg<object[]>(helperCall.Arguments[2]);
            var helper = Arg<HelperDescriptorBase>(helperCall.Object);
            
            return helper.Call(o => o.ReturnInvoke(bindingContext, textWriter, arguments));
        }
    }
}

