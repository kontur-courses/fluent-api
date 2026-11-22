using System;
using System.Globalization;

namespace ObjectPrinting
{
    public sealed class TypeConfig<TOwner, TValue>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PrintingSettings settings;

        internal TypeConfig(
            PrintingConfig<TOwner> printingConfig,
            PrintingSettings settings)
        {
            this.printingConfig = printingConfig;
            this.settings = settings;
        }

        public PrintingConfig<TOwner> Using(
            Func<TValue, string> serializer)
        {
            settings.SetTypeSerializer(serializer);
            return printingConfig;
        }

        public PrintingConfig<TOwner> WithCulture(
            CultureInfo culture)
        {
            settings.SetTypeCulture<TValue>(culture);
            return printingConfig;
        }
    }
}
