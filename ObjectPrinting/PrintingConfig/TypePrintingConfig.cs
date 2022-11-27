using System;
using System.Globalization;

namespace ObjectPrinting.PrintingConfig
{
    public class TypePrintingConfig<TOwner, T> : PrintingConfig<TOwner>
    {
        public TypePrintingConfig(PrintingConfig<TOwner> parent) : base(parent)
        { }

        internal TypePrintingConfig(int maxStringLength, TypePrintingConfig<TOwner, string> parent)
            : base(maxStringLength, parent)
        { }
        
        internal TypePrintingConfig(Type type, CultureInfo cultureInfo, TypePrintingConfig<TOwner, T> parent)
            : base(type, cultureInfo, parent)
        { }
        
        public TypePrintingConfig<TOwner, T> As(Func<T, string> serializer)
        {
            TypeSerializers[typeof(T)] = serializer;
            return this;
        }
    }
}