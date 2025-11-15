using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var serializers = printingConfig.GetTypeSerializers();
            serializers.Add(typeof(TPropType), print);
            
            return printingConfig.BuildPrintingConfig(typeSerializers: serializers);
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(TPropType).IsNumericType())
            {
                throw new InvalidOperationException($"{typeof(TPropType).Name} is not numeric.");
            }

            var serializers = printingConfig.GetTypeSerializers();
            serializers.Add(typeof(TPropType), (TPropType p) => ((IFormattable)p).ToString(null, culture));
            
            return printingConfig.BuildPrintingConfig(typeSerializers: serializers);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}