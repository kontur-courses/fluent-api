using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var objPrint = FuncСonverter.CastToObjectPrint(print);
            if (propertyInfo == null)
            {
                printingConfig.AddTypeAltPrinting(typeof(TPropType), objPrint);
            }
            else
            {
                printingConfig.AddPropAltPrinting(propertyInfo, objPrint);
            }
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var cultureInfo = new CultureInfo(culture.Name);
            var toStringMethod = typeof(TPropType).GetMethod(
                "ToString",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.Any,
                new[] {typeof(CultureInfo)},
                null);
            var print = new Func<TPropType,string>(prop =>
                (string) toStringMethod?.Invoke(prop, new object[]{ cultureInfo }));
            printingConfig.AddTypeAltPrinting(typeof(TPropType), FuncСonverter.CastToObjectPrint(print));
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}