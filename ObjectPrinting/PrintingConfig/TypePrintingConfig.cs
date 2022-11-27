using System;

namespace ObjectPrinting.PrintingConfig
{
    public class TypePrintingConfig<TOwner, T> : PrintingConfig<TOwner>
    {
        public TypePrintingConfig(PrintingConfig<TOwner> parent) : base(parent)
        { }

        internal TypePrintingConfig(int maxStringLength, TypePrintingConfig<TOwner, string> parent)
            : base(maxStringLength, parent)
        { }
        
        public TypePrintingConfig<TOwner, T> As(Func<T, string> serializer)
        {
            TypeSerializers[typeof(T)] = serializer;
            return this;
        }
    }
}