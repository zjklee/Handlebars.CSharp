using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using HandlebarsDotNet.MemberAccessors;

namespace HandlebarsDotNet.ObjectDescriptors
{
    internal class DynamicObjectDescriptor : IObjectDescriptorProvider
    {
        private static readonly DynamicMemberAccessor DynamicMemberAccessor = new DynamicMemberAccessor();
        private static readonly Func<ObjectDescriptor, object, IEnumerable<object>> GetProperties = (descriptor, o) => ((IDynamicMetaObjectProvider) o).GetMetaObject(Expression.Constant(o)).GetDynamicMemberNames();
        private static readonly Type Type = typeof(IDynamicMetaObjectProvider);
        
        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            if (!Type.IsAssignableFrom(type))
            {
                value = ObjectDescriptor.Empty;;
                return false;
            }
            
            value = new ObjectDescriptor(type, DynamicMemberAccessor, GetProperties);

            return true;
        }
    }
}