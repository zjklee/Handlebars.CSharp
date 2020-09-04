using System;
using System.Linq.Expressions;

namespace HandlebarsDotNet
{
    [Flags]
    public enum CompilerFeatures
    {
        None = 0,
        AnonymousTypes = 1
    }
    
    /// <summary>
    /// Executes compilation of lambda <see cref="Expression{T}"/> to actual <see cref="Delegate"/> 
    /// </summary>
    public interface IExpressionCompiler
    {
        CompilerFeatures CompilerFeatures { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Compile<T>(Expression<T> expression) where T: Delegate;
    }

    public interface ICompiler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="requiredFeatures"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Compile<T>(Expression<T> expression, params CompilerFeatures[] requiredFeatures) where T: Delegate;
    }
}