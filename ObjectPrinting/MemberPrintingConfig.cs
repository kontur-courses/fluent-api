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

        public PrintingConfig<TOwner> Using<T>(CultureInfo culture, string format = "") 
            where T: IFormattable
        {
            parentConfig.AddCustomTypeSerializer<T>(o => o.ToString(format, culture));
            return parentConfig;
        }
    }
}