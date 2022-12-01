using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private Type typeConfig;
        public PropertyInfo PropertyConfig { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            typeConfig = typeof(TPropType);
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property) : this(
            printingConfig)
        {
            PropertyConfig = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (PropertyConfig != null)
                printingConfig.AddAlternativeSerializationForProperty(PropertyConfig, obj => print((TPropType)obj));
            if (typeConfig != null)
                printingConfig.AddAlternativeSerializationForType(typeConfig, obj => print((TPropType)obj));
            else
                throw new ArgumentException("Missing type for alternative serialization");
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (typeConfig != null)
                printingConfig.AddCultureForType(typeConfig, culture);
            else
                throw new ArgumentException("Missing type for change culture info");
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}