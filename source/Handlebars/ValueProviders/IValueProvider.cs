using HandlebarsDotNet.Compiler;

namespace HandlebarsDotNet.ValueProviders
{
    internal interface IValueProvider
    {
        void Attach(BindingContext bindingContext);
    }
}