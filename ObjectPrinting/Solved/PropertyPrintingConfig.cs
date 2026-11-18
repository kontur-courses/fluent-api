using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string? memberName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string? memberName)
        {
            this.printingConfig = printingConfig;
            this.memberName = memberName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberName is null)
                printingConfig.TypeSerializers[typeof(TPropType)] = o => print((TPropType)o);
            else
                printingConfig.MemberSerializers[memberName] = o => print((TPropType)o);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                throw new InvalidOperationException(
                    $"Type {typeof(TPropType).Name} does not implement IFormattable.");

            if (memberName is null)
                printingConfig.TypeCultures[typeof(TPropType)] = culture;
            else
                printingConfig.MemberCultures[memberName] = culture;
            return printingConfig;
        }
    }
}