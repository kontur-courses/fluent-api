using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner, TPropertyType> : IPropertyPrintingConfig<TOwner, TPropertyType>
    {
        private readonly PrintingConfig<TOwner> printConfig;
        private readonly PropertyInfo property;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropertyType>.ParentConfig => printConfig;

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropertyType>.Property => property;

        public PropertyConfig(PrintingConfig<TOwner> configs, PropertyInfo property)
        {
            printConfig = configs;
            this.property = property;
        }

        public PrintingConfig<TOwner> SetSerialization(Func<TPropertyType, string> func)
        {
            ((IPrintingConfig<TOwner>)printConfig).PropertyRules.Add(property, x => func((TPropertyType)x));
            return printConfig;
        }
    }

    public interface IPropertyPrintingConfig<TOwner, TPropertyType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }

        PropertyInfo Property { get; }
    }
}