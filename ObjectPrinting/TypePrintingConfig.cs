using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropertyType>
    {
        public TypePrintingConfig(PrintingConfig<TOwner> parent)
        {
            Parent = parent;
        }

        public PrintingConfig<TOwner> Parent { get; }

        public PrintingConfig<TOwner> As(Func<TPropertyType, string> print)
        {
            Parent.AlternativePrintingForTypes[typeof(TPropertyType)] = print;
            return Parent;
        }
    }
}