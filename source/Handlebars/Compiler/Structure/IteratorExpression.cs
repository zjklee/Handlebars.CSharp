﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HandlebarsDotNet.Compiler
{
    internal class IteratorExpression : BlockHelperExpression
    {
        public IteratorExpression(
            string helperName, 
            Expression sequence, 
            BlockParamsExpression blockParams,
            IEnumerable<Expression> arguments,
            Expression template, 
            Expression ifEmpty
        )
            :base(helperName, arguments, blockParams, template, ifEmpty, false)
        {
            Sequence = sequence;
            Template = template;
            IfEmpty = ifEmpty;
        }

        public Expression Sequence { get; }

        public Expression Template { get; }

        public Expression IfEmpty { get; }

        public override Type Type => typeof(void);

        public override ExpressionType NodeType => (ExpressionType)HandlebarsExpressionType.IteratorExpression;
    }
}

