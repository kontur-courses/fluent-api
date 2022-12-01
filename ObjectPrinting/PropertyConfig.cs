using System;
using System.Linq.Expressions;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting
{
    public sealed class PropertyConfig<TOwner, T> : IPropertyConfig<TOwner, T>
    {
        public PropertyConfig(PrintingConfig<TOwner> printer, Expression<Func<TOwner, T>> property)
        {
            Printer = printer;
            PropertyExpression = property;
        }

        public PrintingConfig<TOwner> Printer { get; }
        public Expression<Func<TOwner, T>> PropertyExpression { get; }

        public PrintingConfig<TOwner> UseSerializeMethod(Func<T, string> newMethod)
        {
            Printer.SetSerializeMethodForProperty(PropertyExpression, newMethod);
            return Printer;
        }
    }
}