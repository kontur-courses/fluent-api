using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropertyType>
    {
        public PrintingConfig<TOwner> Parent { get; }


        public PrintingConfig<TOwner> As(Func<TPropertyType, string> print)
        {
            Parent.AlternativePrintingForTypes[typeof(TPropertyType)] = print;
            return Parent;
        }

        public TypePrintingConfig(PrintingConfig<TOwner> parent)
        {
            this.Parent = parent;
        }
    }
}
