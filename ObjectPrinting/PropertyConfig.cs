using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public sealed class PropertyConfig<TOwner, T> : IPropertyConfig<TOwner, T>
    {
        private PrintingConfig<TOwner> printer;
        private Expression<Func<TOwner, T>> property;
        
        public PropertyConfig(PrintingConfig<TOwner> printer, Expression<Func<TOwner, T>> property)
        {
            this.printer = printer;
            this.property = property;
        }

        public IPropertyConfig<TOwner, T> OverrideSerializeMethod(Func<T, string> newMethod)
        {
            printer.OverrideSerializeMethodForProperty(property, newMethod);
            return this;
        }

        public PrintingConfig<TOwner> SetConfig()
        {
            return printer;
        }

        public PrintingConfig<TOwner> ExcludeFromConfig()
        {
            printer.ExcludeProperty(property);
            return printer;
        }
    }
}