using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TProperty>> memberSelector = null)
        {
            if (memberSelector != null && !(memberSelector.Body is MemberExpression))
                throw new ArgumentException("Expression contains not MemberExpression");

            PrintingConfig = printingConfig;
            MemberSelector = memberSelector;
        }

        public PrintingConfig<TOwner> PrintingConfig { get; }
        public Expression<Func<TOwner, TProperty>> MemberSelector { get; }
        public Type PropertyType => typeof(TProperty);

        public PrintingConfig<TOwner> Serialize(Func<TProperty, string> print)
        {
            if (MemberSelector == null)
                PrintingConfig.TypeSerializers[typeof(TProperty)] = print;
            else if (MemberSelector.Body is MemberExpression memberExpression)
                PrintingConfig.PropertySerializers[memberExpression.Member] = print;
            return PrintingConfig;
        }
    }
}