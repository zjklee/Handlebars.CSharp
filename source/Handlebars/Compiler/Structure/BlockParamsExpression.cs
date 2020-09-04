using System;
using System.Linq;
using System.Linq.Expressions;
using HandlebarsDotNet.Polyfills;

namespace HandlebarsDotNet.Compiler
{
    internal class BlockParamsExpression : HandlebarsExpression
    {
        public new static BlockParamsExpression Empty() => new BlockParamsExpression(null);

        public readonly string[] BlockParams;
        
        private BlockParamsExpression(string[] blockParam)
        {
            BlockParams = blockParam ?? ArrayEx.Empty<string>();
        }
        
        public BlockParamsExpression(string action, string blockParams)
            :this(blockParams.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToArray())
        {
        }

        public override ExpressionType NodeType { get; } = (ExpressionType)HandlebarsExpressionType.BlockParamsExpression;

        public override Type Type { get; } = typeof(string[]);

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.Visit(Constant(BlockParams, typeof(string[])));
        }
    }
}