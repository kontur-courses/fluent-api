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
            if (Parent.AlternativePrintingForTypes.ContainsKey(typeof(TPropertyType)))
                throw new InvalidOperationException("An alternative serialization method for this type has already been defined before");

            Parent.AlternativePrintingForTypes[typeof(TPropertyType)] = print;
            return Parent;
        }
    }
}