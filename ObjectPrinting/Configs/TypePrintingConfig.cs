using System;
using System.Globalization;

namespace ObjectPrinting.Configs
{
    public class TypePrintingConfig<TOwner, TType> : ITypePrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }
        public CultureInfo CultureInfo { get; private set; }
        public Func<object, string> Serializer { get; private set; }

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;

            return PrintingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializer)
        {
            Serializer = obj =>
            {
                if (obj is TType value)
                    return serializer(value);

                throw new ArgumentException();
            };

            return PrintingConfig;
        }
    }
}