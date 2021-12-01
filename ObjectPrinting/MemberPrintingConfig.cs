using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TMember, string> alternativeSerializer)
        {
            parentConfig.AddCustomTypeSerializer(alternativeSerializer);
            return parentConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            parentConfig.AddCustomTypeCulture<TMember>(culture);
            return parentConfig;
        }
    }
}