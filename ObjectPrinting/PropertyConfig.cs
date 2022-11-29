using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public sealed class PropertyConfig<TOwner, T> : IPropertyConfig<TOwner, T>
    {
        public PrintingConfig<TOwner> Printer { get; private set; }
        public Expression<Func<TOwner, T>> PropertyExpression { get; private set; }
        
        public PropertyConfig(PrintingConfig<TOwner> printer, Expression<Func<TOwner, T>> property)
        {
            this.Printer = printer;
            this.PropertyExpression = property;
        }

        public IPropertyConfig<TOwner, T> OverrideSerializeMethod(Func<T, string> newMethod)
        {
            Printer.OverrideSerializeMethodForProperty(PropertyExpression, newMethod);
            return this;
        }

        public PrintingConfig<TOwner> SetConfig()
        {
            return Printer;
        }

        public PrintingConfig<TOwner> ExcludeFromConfig()
        {
            Printer.ExcludeProperty(PropertyExpression);
            return Printer;
        }
    }
}