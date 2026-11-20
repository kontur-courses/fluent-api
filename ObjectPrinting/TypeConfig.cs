using System;
using System.Globalization;

namespace ObjectPrinting
{
    public sealed class TypeConfig<TOwner, TValue>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        internal TypeConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(
            Func<TValue, string> serializer)
        {
            printingConfig.SetTypeSerializer(serializer);
            return printingConfig;
        }

        public PrintingConfig<TOwner> WithCulture(
            CultureInfo culture)
        {
            printingConfig.SetTypeCulture<TValue>(culture);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Excluding()
        {
            printingConfig.SetExcludingType<TValue>();
            return printingConfig;
        }
    }
}