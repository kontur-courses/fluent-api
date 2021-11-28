using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType> : IMemberPrintingConfig<TOwner, TType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            ParentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializationRule)
        {
            return new PrintingConfig<TOwner>(ParentConfig, typeof(TType), serializationRule);
        }
    }
}
