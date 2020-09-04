using System.Runtime.CompilerServices;
using HandlebarsDotNet.ObjectDescriptors;

namespace HandlebarsDotNet.Compiler.Structure.Path
{
    internal static class PathResolver
    {
        public static object ResolvePath(BindingContext context, PathInfo pathInfo)
        {
            if (!pathInfo.HasValue) return null;
            
            var instance = context.Value;

            if (pathInfo.HasContextChange)
            {
                for (var i = 0; i < pathInfo.ContextChangeDepth; i++)
                {
                    context = context.ParentContext;
                    if (context == null)
                    {
                        if (!pathInfo.IsVariable)
                            throw new HandlebarsCompilerException("Path expression tried to reference parent of root");
                        
                        return string.Empty;
                    }

                    instance = context.Value;
                }
            }

            if (pathInfo.IsPureThis) return instance;

            var hashParameters = instance as HashParameterDictionary;
            
            var pathChain = pathInfo.PathChain;
            
            for (var index = 0; index < pathChain.Length; index++)
            {
                var chainSegment = pathChain[index];
                instance = ResolveValue(context, instance, chainSegment);

                if (!(instance is UndefinedBindingResult))
                {
                    continue;
                }

                if (hashParameters == null || hashParameters.ContainsKey(chainSegment) || context.ParentContext == null)
                {
                    if (context.Configuration.ThrowOnUnresolvedBindingExpression) 
                        throw new HandlebarsUndefinedBindingException(pathInfo, (instance as UndefinedBindingResult).Value);
                    
                    return instance;
                }

                instance = ResolveValue(context.ParentContext, context.ParentContext.Value, chainSegment);
                if (!(instance is UndefinedBindingResult result))
                {
                    continue;
                }

                if (context.Configuration.ThrowOnUnresolvedBindingExpression)
                    throw new HandlebarsUndefinedBindingException(pathInfo, result.Value);
                
                return instance;
            }
            
            return instance;
        }
        
        public static object ResolveValue(BindingContext context, object instance, ChainSegment chainSegment)
        {
            if (instance == null)
            {
                return chainSegment.GetUndefinedBindingResult(context.Configuration);
            }
            
            if (chainSegment.IsThis && !chainSegment.IsVariable) return instance;
            
            object resolvedValue;
            if (chainSegment.IsVariable)
            {
                return context.TryGetContextVariable(chainSegment, out resolvedValue)
                    ? resolvedValue
                    : chainSegment.GetUndefinedBindingResult(context.Configuration);
            }
            
            if (context.TryGetVariable(chainSegment, out resolvedValue)
                || TryAccessMember(instance, chainSegment, context.Configuration, out resolvedValue))
            {
                return resolvedValue;
            }
            
            if (chainSegment.Equals("value"))
            {
                if (context.TryGetContextVariable(chainSegment, out resolvedValue))
                {
                    return resolvedValue;
                }
                
                return context.Value;
            }

            return chainSegment.GetUndefinedBindingResult(context.Configuration);
        }

        public static bool TryAccessMember(object instance, ChainSegment chainSegment, ICompiledHandlebarsConfiguration configuration, out object value)
        {
            if (instance == null)
            {
                value = chainSegment.GetUndefinedBindingResult(configuration);
                return false;
            }

            var typeDescriptor = TypeDescriptor.Create(instance, configuration);
            chainSegment = ResolveMemberName(instance, chainSegment, configuration);

            if (!typeDescriptor.TryGetObjectDescriptor(out var descriptor))
            {
                value = chainSegment.GetUndefinedBindingResult(configuration);
                return false;
            }

            return descriptor.MemberAccessor.TryGetValue(instance, typeDescriptor.Type, chainSegment, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ChainSegment ResolveMemberName(object instance, ChainSegment memberName,
            ICompiledHandlebarsConfiguration configuration)
        {
            var resolver = configuration.ExpressionNameResolver;
            if (resolver == null) return memberName;

            return resolver.ResolveExpressionName(instance, memberName.TrimmedValue);
        }
    }
}