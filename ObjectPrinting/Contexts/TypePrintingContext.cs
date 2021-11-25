using System;

namespace ObjectPrinting.Contexts
{
    public class TypePrintingContext<TOwner, TPropType>
    {
        private readonly PrintingConfig config;

        public TypePrintingContext(PrintingConfig config)
        {
            this.config = config;
        }

        public ConfigPrintingContext<TOwner> Using(Func<TPropType, string> print)
        {
            return new ConfigPrintingContext<TOwner>(config with
            {
                TypePrinting = config.TypePrinting
                    .SetItem(typeof(TPropType), obj => print((TPropType)obj))
            });
        }
    }
}