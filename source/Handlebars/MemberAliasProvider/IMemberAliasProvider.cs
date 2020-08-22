using System;
using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet
{
    /// <summary>
    /// Allows to redirect member access to a different member
    /// </summary>
    public interface IMemberAliasProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="targetType"></param>
        /// <param name="memberAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetMemberByAlias(object instance, Type targetType, ChainSegment memberAlias, out object value);
    }
}