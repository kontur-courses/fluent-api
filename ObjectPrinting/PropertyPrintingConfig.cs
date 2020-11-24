using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TProperty, TOwner>
    {
        public PropertyPrintingConfig(PrintingConfig<TOwner> config, string propertyName)
        {
            Config = config;
            Property = propertyName;
        }

        private PrintingConfig<TOwner> Config { get; }
        private string Property { get; }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> func)
        {
            Config.AddSerialization(Property, func);

            return Config;
        }
    }
}