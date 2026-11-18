using ObjectPrinting.Solved.Interfaces;
using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class TypePrinting<TOwner, TPropType> : ITypePrinting<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> parent;
        public TypePrinting(PrintingConfig<TOwner> parent) => this.parent = parent;
        public PrintingConfig<TOwner> ParentConfig => parent;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            parent.TypeSerializers[typeof(TPropType)] = o => serializer((TPropType)o);
            return parent;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                throw new InvalidOperationException(
                    $"Type {typeof(TPropType).Name} does not implement IFormattable.");
            parent.TypeCultures[typeof(TPropType)] = culture;
            return parent;
        }
    }
}