using System;
using System.Reflection;

namespace ObjectPrinting.Config.Property
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public readonly PropertyInfo PropertyToChange;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyToChange)
        {
            ParentConfig = printingConfig;
            PropertyToChange = propertyToChange;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            ParentConfig.OverridePropertyPrinting(PropertyToChange, obj => print((TPropType) obj));

            return ParentConfig;
        }
    }
}