using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public CustomSerializablePrintingConfig<TOwner> CustomSerializablePrintingConfig { get; }
        public Expression<Func<TOwner, TProperty>>  MemberSelector { get; }
        public Type PropertyType => typeof(TProperty);
        

        public PropertyPrintingConfig(CustomSerializablePrintingConfig<TOwner> customSerializablePrintingConfig, Expression<Func<TOwner, TProperty>> memberSelector = null)
        {
            CustomSerializablePrintingConfig = customSerializablePrintingConfig;
            MemberSelector = memberSelector;
        }

        public CustomSerializablePrintingConfig<TOwner> Serialize(Func<TProperty, string> print)
        {
            if (MemberSelector == null)
                ((ICustomSerializablePrintingConfig) CustomSerializablePrintingConfig).TypeSerializers[typeof(TProperty)] = print;
            else if (MemberSelector.Body is MemberExpression memberExpression)
                ((ICustomSerializablePrintingConfig) CustomSerializablePrintingConfig).PropertySerializers[memberExpression.Member] = print;
            return CustomSerializablePrintingConfig;
        }
    }
}