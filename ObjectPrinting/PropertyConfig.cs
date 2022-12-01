using System;
using System.Linq.Expressions;
using ObjectPrinting.PrintingConfig;

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

        public PrintingConfig<TOwner> UseSerializeMethod(Func<T, string> newMethod)
        {
            Printer.SetSerializeMethodForProperty(PropertyExpression, newMethod);
            return Printer;
        }
    }
}