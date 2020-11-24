using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropertyType>
    {
        public TypePrintingConfig(PrintingConfig<TOwner> config)
        {
            Config = config;
        }

        private PrintingConfig<TOwner> Config { get; }

        public PrintingConfig<TOwner> Using(Func<TPropertyType, string> func)
        {
            Config.AddSerialization(typeof(TPropertyType), func);

            return Config;
        }
    }
}