using System;
using System.IO;
using System.Runtime.CompilerServices;
using HandlebarsDotNet.ObjectDescriptors;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet.Iterators
{
    public abstract class Iterator
    {
        protected readonly ObjectDescriptor Descriptor;
        
        protected Iterator(ObjectDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables, object target, Action<BindingContext, TextWriter, object> template, Action<BindingContext, TextWriter, object> ifEmpty);
    }
    
    public abstract class Iterator<T> : Iterator
    {
        protected Iterator(ObjectDescriptor descriptor) : base(descriptor)
        {
        }
        
        public abstract void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables, T target, Action<BindingContext, TextWriter, object> template, Action<BindingContext, TextWriter, object> ifEmpty);

        public sealed override void Iterate(BindingContext context, BlockParamsVariables blockParamsVariables, object target, Action<BindingContext, TextWriter, object> template, Action<BindingContext, TextWriter, object> ifEmpty)
        {
            Iterate(context, blockParamsVariables, (T) target, template, ifEmpty);
        }
    }
}