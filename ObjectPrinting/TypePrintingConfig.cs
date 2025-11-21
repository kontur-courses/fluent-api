using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            printingConfig.AddTypeSerializer(serializer);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
  
            if (typeof(TPropType) == typeof(double))
            {
                printingConfig.AddTypeSerializer<double>(d => d.ToString(culture));
            }
            else if (typeof(TPropType) == typeof(float))
            {
                printingConfig.AddTypeSerializer<float>(f => f.ToString(culture));
            }
            else if (typeof(TPropType) == typeof(int))
            {
                printingConfig.AddTypeSerializer<int>(i => i.ToString(culture));
            }

            
            return printingConfig;
        }
    }
}